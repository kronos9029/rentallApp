using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RentalApp.Application.Features.Booking;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;
using BookingEntity = RentalApp.Domain.Entities.Booking;

namespace RentalApp.Infrastructure.Booking;

public sealed class CheckoutService(
    RentalAppDbContext dbContext,
    DeadlockRetryExecutor retryExecutor,
    IHoldLifecycleService holdLifecycleService,
    TimeProvider timeProvider) : ICheckoutService
{
    private static readonly JsonSerializerOptions SnapshotSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<CheckoutPreview?> GetPreviewAsync(string holdId, string userId, CancellationToken cancellationToken = default)
    {
        await holdLifecycleService.ExpireHoldIfNeededAsync(holdId, cancellationToken);

        var booking = await dbContext.Bookings
            .AsNoTracking()
            .Where(entity => entity.HoldId == holdId && entity.UserId == userId)
            .Include(entity => entity.BookingItems)
                .ThenInclude(item => item.Court)
            .Include(entity => entity.BookingItems)
                .ThenInclude(item => item.TimeBucket)
            .SingleOrDefaultAsync(cancellationToken);

        if (booking is not null)
        {
            var existingOrderId = await dbContext.CheckoutOrderItems
                .AsNoTracking()
                .Where(item => item.BookingId == booking.BookingId)
                .Select(item => item.OrderId)
                .SingleOrDefaultAsync(cancellationToken);

            return new CheckoutPreview(
                holdId,
                HoldStatus.Converted,
                booking.EndAt,
                booking.TotalAmount,
                false,
                existingOrderId,
                booking.BookingItems
                    .OrderBy(item => item.TimeBucket.StartAt)
                    .Select(MapCheckoutItem)
                    .ToArray(),
                "Checkout da duoc tao truoc do.");
        }

        var hold = await dbContext.Holds
            .AsNoTracking()
            .Where(entity => entity.HoldId == holdId && entity.UserId == userId)
            .Include(entity => entity.HoldItems)
                .ThenInclude(item => item.Court)
            .Include(entity => entity.HoldItems)
                .ThenInclude(item => item.TimeBucket)
            .SingleOrDefaultAsync(cancellationToken);

        if (hold is null)
        {
            return null;
        }

        var errorMessage = hold.Status switch
        {
            HoldStatus.Expired => "Hold da het han. Vui long quay lai availability de tao hold moi.",
            HoldStatus.Cancelled => "Hold da bi huy.",
            _ => null
        };

        return new CheckoutPreview(
            hold.HoldId,
            hold.Status,
            hold.ExpiresAt,
            hold.HoldItems.Sum(item => item.LineTotal),
            hold.Status == HoldStatus.Active,
            null,
            hold.HoldItems
                .OrderBy(item => item.TimeBucket.StartAt)
                .Select(item => new CheckoutItemSummary(
                    item.HoldItemId,
                    item.CourtId,
                    item.Court.CourtCode,
                    item.Court.CourtName,
                    item.BucketId,
                    item.TimeBucket.StartAt,
                    item.TimeBucket.EndAt,
                    item.BookingMode,
                    item.SlotQty,
                    item.UnitPrice,
                    item.LineTotal))
                .ToArray(),
            errorMessage);
    }

    public Task<CheckoutOperationResult> CreateAsync(CreateCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        return retryExecutor.ExecuteAsync(
            async retryToken =>
            {
                await holdLifecycleService.ExpireHoldIfNeededAsync(request.HoldId, retryToken);

                const string endpoint = "/api/checkout";
                var requestHash = ComputeRequestHash(request.HoldId);

                var existingIdempotency = await dbContext.IdempotencyKeys
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        record => record.Scope == request.UserId &&
                                  record.Endpoint == endpoint &&
                                  record.IdempotencyKeyValue == request.IdempotencyKey,
                        retryToken);

                if (existingIdempotency is not null)
                {
                    if (!string.Equals(existingIdempotency.RequestHash, requestHash, StringComparison.Ordinal))
                    {
                        return new CheckoutOperationResult(false, null, "Idempotency-Key da duoc dung voi payload khac.");
                    }

                    var replayedOrder = JsonSerializer.Deserialize<CheckoutOrderSummary>(
                        existingIdempotency.ResponseSnapshot,
                        SnapshotSerializerOptions);

                    return new CheckoutOperationResult(true, replayedOrder, Replayed: true);
                }

                var useTransaction = dbContext.Database.IsRelational();
                await using var transaction = useTransaction
                    ? await dbContext.Database.BeginTransactionAsync(retryToken)
                    : null;

                var hold = useTransaction
                    ? await dbContext.Holds
                        .FromSqlInterpolated($"SELECT * FROM holds WHERE hold_id = {request.HoldId} FOR UPDATE")
                        .Include(entity => entity.HoldItems)
                            .ThenInclude(item => item.Court)
                        .Include(entity => entity.HoldItems)
                            .ThenInclude(item => item.TimeBucket)
                        .SingleOrDefaultAsync(entity => entity.UserId == request.UserId, retryToken)
                    : await dbContext.Holds
                        .Include(entity => entity.HoldItems)
                            .ThenInclude(item => item.Court)
                        .Include(entity => entity.HoldItems)
                            .ThenInclude(item => item.TimeBucket)
                        .SingleOrDefaultAsync(entity => entity.HoldId == request.HoldId && entity.UserId == request.UserId, retryToken);

                if (hold is null)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(retryToken);
                    }
                    return new CheckoutOperationResult(false, null, "Khong tim thay hold de checkout.");
                }

                if (hold.Status == HoldStatus.Expired || hold.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(retryToken);
                    }
                    return new CheckoutOperationResult(false, null, "Hold da het han. Vui long tao hold moi.", Conflict: true);
                }

                var existingBooking = await dbContext.Bookings
                    .Include(entity => entity.BookingItems)
                        .ThenInclude(item => item.Court)
                    .Include(entity => entity.BookingItems)
                        .ThenInclude(item => item.TimeBucket)
                    .Where(entity => entity.HoldId == hold.HoldId)
                    .SingleOrDefaultAsync(retryToken);

                if (existingBooking is not null)
                {
                    var existingOrderId = await dbContext.CheckoutOrderItems
                        .AsNoTracking()
                        .Where(item => item.BookingId == existingBooking.BookingId)
                        .Select(item => item.OrderId)
                        .SingleAsync(retryToken);

                    var existingOrder = await dbContext.CheckoutOrders
                        .AsNoTracking()
                        .SingleAsync(order => order.OrderId == existingOrderId, retryToken);

                    var summary = BuildOrderSummary(existingOrder, existingBooking, existingBooking.BookingItems);
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(retryToken);
                    }
                    return new CheckoutOperationResult(true, summary, Replayed: true);
                }

                if (hold.Status != HoldStatus.Active)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(retryToken);
                    }
                    return new CheckoutOperationResult(false, null, "Hold khong con hop le cho checkout.", Conflict: true);
                }

                var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
                var orderedItems = hold.HoldItems.OrderBy(item => item.TimeBucket.StartAt).ToArray();
                var bookingId = Guid.NewGuid().ToString();
                var orderId = Guid.NewGuid().ToString();
                var booking = new BookingEntity
                {
                    BookingId = bookingId,
                    UserId = request.UserId,
                    HoldId = hold.HoldId,
                    Status = BookingStatus.Pending,
                    TotalAmount = orderedItems.Sum(item => item.LineTotal),
                    BookingDate = DateOnly.FromDateTime(orderedItems.Min(item => item.TimeBucket.StartAt)),
                    StartAt = orderedItems.Min(item => item.TimeBucket.StartAt),
                    EndAt = orderedItems.Max(item => item.TimeBucket.EndAt),
                    CancellationPolicyId = await GetActiveCancellationPolicyIdAsync(retryToken),
                    CreatedAt = nowUtc,
                    UpdatedAt = nowUtc
                };

                var bookingItems = orderedItems
                    .Select(item => new BookingItem
                    {
                        BookingItemId = Guid.NewGuid().ToString(),
                        BookingId = bookingId,
                        CourtId = item.CourtId,
                        BucketId = item.BucketId,
                        BookingMode = item.BookingMode,
                        SlotQty = item.SlotQty,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal,
                        Status = BookingItemStatus.Active,
                        CreatedAt = nowUtc,
                        UpdatedAt = nowUtc
                    })
                    .ToArray();

                var order = new CheckoutOrder
                {
                    OrderId = orderId,
                    UserId = request.UserId,
                    Status = CheckoutOrderStatus.Pending,
                    SubtotalAmount = booking.TotalAmount,
                    DiscountAmount = 0m,
                    TotalAmount = booking.TotalAmount,
                    Currency = "VND",
                    ExpiresAt = hold.ExpiresAt,
                    CreatedAt = nowUtc,
                    UpdatedAt = nowUtc
                };

                var orderItem = new CheckoutOrderItem
                {
                    OrderItemId = Guid.NewGuid().ToString(),
                    OrderId = orderId,
                    BookingId = bookingId,
                    CreatedAt = nowUtc
                };

                hold.Status = HoldStatus.Converted;
                hold.UpdatedAt = nowUtc;

                await dbContext.Bookings.AddAsync(booking, retryToken);
                await dbContext.BookingItems.AddRangeAsync(bookingItems, retryToken);
                await dbContext.CheckoutOrders.AddAsync(order, retryToken);
                await dbContext.CheckoutOrderItems.AddAsync(orderItem, retryToken);

                var summarySnapshot = BuildOrderSummary(order, booking, bookingItems);
                await dbContext.IdempotencyKeys.AddAsync(
                    new IdempotencyKey
                    {
                        IdempotencyId = Guid.NewGuid().ToString(),
                        Scope = request.UserId,
                        Endpoint = endpoint,
                        IdempotencyKeyValue = request.IdempotencyKey,
                        RequestHash = requestHash,
                        ResponseStatus = 201,
                        ResponseSnapshot = JsonSerializer.Serialize(summarySnapshot, SnapshotSerializerOptions),
                        ExpiresAt = hold.ExpiresAt,
                        CreatedAt = nowUtc
                    },
                    retryToken);

                await dbContext.SaveChangesAsync(retryToken);
                if (transaction is not null)
                {
                    await transaction.CommitAsync(retryToken);
                }

                return new CheckoutOperationResult(true, summarySnapshot);
            },
            cancellationToken);
    }

    private async Task<string?> GetActiveCancellationPolicyIdAsync(CancellationToken cancellationToken)
    {
        return await dbContext.CancellationPolicies
            .AsNoTracking()
            .Where(policy => policy.IsActive)
            .OrderBy(policy => policy.MinHoursBeforeStart)
            .Select(policy => policy.PolicyId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static CheckoutItemSummary MapCheckoutItem(BookingItem item)
    {
        return new CheckoutItemSummary(
            item.BookingItemId,
            item.CourtId,
            item.Court.CourtCode,
            item.Court.CourtName,
            item.BucketId,
            item.TimeBucket.StartAt,
            item.TimeBucket.EndAt,
            item.BookingMode,
            item.SlotQty,
            item.UnitPrice,
            item.LineTotal);
    }

    private static CheckoutOrderSummary BuildOrderSummary(
        CheckoutOrder order,
        BookingEntity booking,
        IEnumerable<BookingItem> bookingItems)
    {
        var items = bookingItems
            .OrderBy(item => item.TimeBucket.StartAt)
            .Select(MapCheckoutItem)
            .ToArray();

        return new CheckoutOrderSummary(
            order.OrderId,
            booking.BookingId,
            booking.HoldId ?? string.Empty,
            order.Status,
            order.TotalAmount,
            order.Currency,
            order.ExpiresAt,
            order.CreatedAt,
            items);
    }

    private static string ComputeRequestHash(string holdId)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(holdId));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static void ValidateRequest(CreateCheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new InvalidOperationException("Authenticated user id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.HoldId))
        {
            throw new InvalidOperationException("Hold id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new InvalidOperationException("Idempotency-Key is required.");
        }
    }
}
