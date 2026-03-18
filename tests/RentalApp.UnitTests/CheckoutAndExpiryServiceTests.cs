using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RentalApp.Application.Features.Availability;
using RentalApp.Application.Features.Booking;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Booking;
using RentalApp.Infrastructure.Persistence;
using RentalApp.Infrastructure.Persistence.Repositories;
using RentalApp.Infrastructure.Persistence.Seed;

namespace RentalApp.UnitTests;

public sealed class CheckoutAndExpiryServiceTests
{
    [Fact]
    public async Task ExpireExpiredHoldsAsync_ReleasesInventoryAndMarksHoldExpired()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 18, 12, 0, 0, TimeSpan.Zero));
        await using var dbContext = CreateDbContext();
        await SeedFoundationAsync(dbContext);

        var availabilityService = new AvailabilityReadService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, timeProvider),
            new PricingRuleReadService(dbContext),
            new MemoryCache(Options.Create(new MemoryCacheOptions())));

        var availability = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(new DateOnly(2026, 3, 20), BookingMode.Shared, 3));

        var court = availability.Courts.First();
        var bucketId = court.Slots.First(slot => slot.IsAvailable).BucketId;
        var holdLifecycleService = new HoldLifecycleService(dbContext, new DeadlockRetryExecutor(), timeProvider);
        var holdService = new HoldService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, timeProvider),
            new PricingRuleReadService(dbContext),
            holdLifecycleService,
            new DeadlockRetryExecutor(),
            timeProvider);

        var holdResult = await holdService.CreateAsync(
            new CreateHoldRequest(
                AppSeedData.SeedCustomerUserId,
                court.CourtId,
                [bucketId],
                BookingMode.Shared,
                3,
                "expire-shared-key"));

        Assert.True(holdResult.Success);

        timeProvider.Advance(TimeSpan.FromMinutes(16));

        var expiredCount = await holdLifecycleService.ExpireExpiredHoldsAsync();

        Assert.Equal(1, expiredCount);

        var hold = await dbContext.Holds.SingleAsync(entity => entity.HoldId == holdResult.Hold!.HoldId);
        var bucket = await dbContext.CourtBuckets.SingleAsync(entity => entity.CourtId == court.CourtId && entity.BucketId == bucketId);

        Assert.Equal(HoldStatus.Expired, hold.Status);
        Assert.Equal(CourtBucketMode.None, bucket.Mode);
        Assert.Equal(0, bucket.SharedReserved);
    }

    [Fact]
    public async Task CreateAsync_CreatesPendingBookingAndCheckoutOrder()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 18, 12, 0, 0, TimeSpan.Zero));
        await using var dbContext = CreateDbContext();
        await SeedFoundationAsync(dbContext);

        var availabilityService = new AvailabilityReadService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, timeProvider),
            new PricingRuleReadService(dbContext),
            new MemoryCache(Options.Create(new MemoryCacheOptions())));

        var availability = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(new DateOnly(2026, 3, 20), BookingMode.Private, 1));

        var court = availability.Courts.First();
        var bucketId = court.Slots.First(slot => slot.IsAvailable).BucketId;
        var holdLifecycleService = new HoldLifecycleService(dbContext, new DeadlockRetryExecutor(), timeProvider);
        var holdService = new HoldService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, timeProvider),
            new PricingRuleReadService(dbContext),
            holdLifecycleService,
            new DeadlockRetryExecutor(),
            timeProvider);

        var holdResult = await holdService.CreateAsync(
            new CreateHoldRequest(
                AppSeedData.SeedCustomerUserId,
                court.CourtId,
                [bucketId],
                BookingMode.Private,
                1,
                "checkout-hold-key"));

        var checkoutService = new CheckoutService(
            dbContext,
            new DeadlockRetryExecutor(),
            holdLifecycleService,
            timeProvider);

        var checkoutResult = await checkoutService.CreateAsync(
            new CreateCheckoutRequest(
                AppSeedData.SeedCustomerUserId,
                holdResult.Hold!.HoldId,
                "checkout-key-1"));

        Assert.True(checkoutResult.Success);
        Assert.NotNull(checkoutResult.Order);

        var hold = await dbContext.Holds.SingleAsync(entity => entity.HoldId == holdResult.Hold.HoldId);
        var booking = await dbContext.Bookings.Include(entity => entity.BookingItems).SingleAsync();
        var order = await dbContext.CheckoutOrders.SingleAsync();

        Assert.Equal(HoldStatus.Converted, hold.Status);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Single(booking.BookingItems);
        Assert.Equal(CheckoutOrderStatus.Pending, order.Status);
        Assert.Equal(booking.TotalAmount, order.TotalAmount);
    }

    [Fact]
    public async Task CreateAsync_ReplaysCheckoutIdempotencyKey()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 18, 12, 0, 0, TimeSpan.Zero));
        await using var dbContext = CreateDbContext();
        await SeedFoundationAsync(dbContext);

        var availabilityService = new AvailabilityReadService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, timeProvider),
            new PricingRuleReadService(dbContext),
            new MemoryCache(Options.Create(new MemoryCacheOptions())));

        var availability = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(new DateOnly(2026, 3, 21), BookingMode.Shared, 2));

        var court = availability.Courts.First();
        var bucketIds = court.Slots.Where(slot => slot.IsAvailable).Take(2).Select(slot => slot.BucketId).ToArray();
        var holdLifecycleService = new HoldLifecycleService(dbContext, new DeadlockRetryExecutor(), timeProvider);
        var holdService = new HoldService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, timeProvider),
            new PricingRuleReadService(dbContext),
            holdLifecycleService,
            new DeadlockRetryExecutor(),
            timeProvider);

        var holdResult = await holdService.CreateAsync(
            new CreateHoldRequest(
                AppSeedData.SeedCustomerUserId,
                court.CourtId,
                bucketIds,
                BookingMode.Shared,
                2,
                "checkout-hold-idempo"));

        var checkoutService = new CheckoutService(
            dbContext,
            new DeadlockRetryExecutor(),
            holdLifecycleService,
            timeProvider);

        var request = new CreateCheckoutRequest(AppSeedData.SeedCustomerUserId, holdResult.Hold!.HoldId, "checkout-idempo-1");

        var firstResult = await checkoutService.CreateAsync(request);
        var replayResult = await checkoutService.CreateAsync(request);

        Assert.True(firstResult.Success);
        Assert.True(replayResult.Success);
        Assert.True(replayResult.Replayed);
        Assert.Equal(firstResult.Order!.OrderId, replayResult.Order!.OrderId);
    }

    private static RentalAppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RentalAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new RentalAppDbContext(options);
    }

    private static async Task SeedFoundationAsync(RentalAppDbContext dbContext)
    {
        await dbContext.Users.AddRangeAsync(
            CloneUser(AppSeedData.SeedAdminUser),
            CloneUser(AppSeedData.SeedCustomerUser));

        await dbContext.Courts.AddRangeAsync(AppSeedData.Courts.Select(CloneCourt));
        await dbContext.PricingRules.AddRangeAsync(AppSeedData.PricingRules.Select(ClonePricingRule));
        await dbContext.CancellationPolicies.AddAsync(CloneCancellationPolicy(AppSeedData.StandardCancellationPolicy));
        await dbContext.SaveChangesAsync();
    }

    private static AppUser CloneUser(AppUser user)
    {
        return new AppUser
        {
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private static Court CloneCourt(Court court)
    {
        return new Court
        {
            CourtId = court.CourtId,
            CourtCode = court.CourtCode,
            CourtName = court.CourtName,
            SortOrder = court.SortOrder,
            IsActive = court.IsActive,
            MaintenanceReason = court.MaintenanceReason,
            CreatedAt = court.CreatedAt,
            UpdatedAt = court.UpdatedAt
        };
    }

    private static PricingRule ClonePricingRule(PricingRule rule)
    {
        return new PricingRule
        {
            PricingRuleId = rule.PricingRuleId,
            RuleName = rule.RuleName,
            AppliesTo = rule.AppliesTo,
            DayType = rule.DayType,
            StartTime = rule.StartTime,
            EndTime = rule.EndTime,
            PrivateRate = rule.PrivateRate,
            SharedRate = rule.SharedRate,
            WeekendMarkupPct = rule.WeekendMarkupPct,
            IsActive = rule.IsActive,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            CreatedByUserId = rule.CreatedByUserId,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }

    private static CancellationPolicy CloneCancellationPolicy(CancellationPolicy policy)
    {
        return new CancellationPolicy
        {
            PolicyId = policy.PolicyId,
            PolicyName = policy.PolicyName,
            MinHoursBeforeStart = policy.MinHoursBeforeStart,
            RefundStrategy = policy.RefundStrategy,
            RefundPercent = policy.RefundPercent,
            CancelFeePercent = policy.CancelFeePercent,
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan delta) => _utcNow = _utcNow.Add(delta);
    }
}
