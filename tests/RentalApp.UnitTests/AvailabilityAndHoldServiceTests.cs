using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentalApp.Application.Features.Availability;
using RentalApp.Application.Features.Booking;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Booking;
using RentalApp.Infrastructure.Persistence;
using RentalApp.Infrastructure.Persistence.Repositories;
using RentalApp.Infrastructure.Persistence.Seed;

namespace RentalApp.UnitTests;

public sealed class AvailabilityAndHoldServiceTests
{
    [Fact]
    public async Task GetAvailabilityAsync_ProvisionInventoryAndReturnCourtSlots()
    {
        await using var dbContext = CreateDbContext();
        await SeedFoundationAsync(dbContext);
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var availabilityService = new AvailabilityReadService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, TimeProvider.System),
            new PricingRuleReadService(dbContext),
            memoryCache);

        var result = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(new DateOnly(2026, 3, 19), BookingMode.Private, 1));

        Assert.Equal(9, result.Courts.Count);
        Assert.All(result.Courts, court =>
        {
            Assert.Equal(16, court.Slots.Count);
            Assert.All(court.Slots, slot => Assert.True(slot.IsAvailable));
        });
    }

    [Fact]
    public async Task CreateAsync_CreatesHoldAndReplaysIdempotentRequest()
    {
        await using var dbContext = CreateDbContext();
        await SeedFoundationAsync(dbContext);
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var availabilityService = new AvailabilityReadService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, TimeProvider.System),
            new PricingRuleReadService(dbContext),
            memoryCache);

        var availability = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(new DateOnly(2026, 3, 21), BookingMode.Shared, 2));

        var court = availability.Courts.First();
        var bucketIds = court.Slots.Where(slot => slot.IsAvailable).Take(2).Select(slot => slot.BucketId).ToArray();

        var holdService = new HoldService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, TimeProvider.System),
            new PricingRuleReadService(dbContext),
            new HoldLifecycleService(dbContext, new DeadlockRetryExecutor(), TimeProvider.System),
            new DeadlockRetryExecutor(),
            TimeProvider.System);

        var request = new CreateHoldRequest(
            AppSeedData.SeedCustomerUserId,
            court.CourtId,
            bucketIds,
            BookingMode.Shared,
            2,
            "hold-key-1");

        var firstResult = await holdService.CreateAsync(request);
        var replayResult = await holdService.CreateAsync(request);

        Assert.True(firstResult.Success);
        Assert.NotNull(firstResult.Hold);
        Assert.Equal(2, firstResult.Hold!.Items.Count);
        Assert.True(firstResult.Hold.TotalAmount > 0);

        Assert.True(replayResult.Success);
        Assert.True(replayResult.Replayed);
        Assert.NotNull(replayResult.Hold);
        Assert.Equal(firstResult.Hold.HoldId, replayResult.Hold!.HoldId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenPrivateBucketAlreadyHeld()
    {
        await using var dbContext = CreateDbContext();
        await SeedFoundationAsync(dbContext);
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var availabilityService = new AvailabilityReadService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, TimeProvider.System),
            new PricingRuleReadService(dbContext),
            memoryCache);

        var availability = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(new DateOnly(2026, 3, 20), BookingMode.Private, 1));

        var court = availability.Courts.First();
        var bucketId = court.Slots.First(slot => slot.IsAvailable).BucketId;

        var holdService = new HoldService(
            dbContext,
            new BookingInventoryProvisioner(dbContext, TimeProvider.System),
            new PricingRuleReadService(dbContext),
            new HoldLifecycleService(dbContext, new DeadlockRetryExecutor(), TimeProvider.System),
            new DeadlockRetryExecutor(),
            TimeProvider.System);

        var firstResult = await holdService.CreateAsync(
            new CreateHoldRequest(
                AppSeedData.SeedCustomerUserId,
                court.CourtId,
                [bucketId],
                BookingMode.Private,
                1,
                "private-key-1"));

        var secondResult = await holdService.CreateAsync(
            new CreateHoldRequest(
                AppSeedData.SeedAdminUserId,
                court.CourtId,
                [bucketId],
                BookingMode.Private,
                1,
                "private-key-2"));

        Assert.True(firstResult.Success);
        Assert.False(secondResult.Success);
        Assert.True(secondResult.Conflict);
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
}
