using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RentalApp.Application.Features.Booking;
using RentalApp.Application.Features.Pricing;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;

namespace RentalApp.Infrastructure.Booking;

public sealed class HoldService(
    RentalAppDbContext dbContext,
    BookingInventoryProvisioner inventoryProvisioner,
    IPriceCalculationService priceCalculationService,
    IHoldLifecycleService holdLifecycleService,
    DeadlockRetryExecutor retryExecutor,
    TimeProvider timeProvider) : IHoldService
{
    private static readonly JsonSerializerOptions SnapshotSerializerOptions = new(JsonSerializerDefaults.Web);

    public Task<HoldOperationResult> CreateAsync(CreateHoldRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        return retryExecutor.ExecuteAsync(
            async retryToken =>
            {
                var canonicalBucketIds = request.BucketIds
                    .Where(bucketId => !string.IsNullOrWhiteSpace(bucketId))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(bucketId => bucketId, StringComparer.Ordinal)
                    .ToArray();
                var bucketIdList = canonicalBucketIds.ToList();

                if (canonicalBucketIds.Length == 0)
                {
                    return new HoldOperationResult(false, null, "Ban can chon it nhat mot khung gio.");
                }

                var bookingDate = await ResolveBookingDateAsync(bucketIdList, retryToken);
                await inventoryProvisioner.EnsureDailyInventoryAsync(bookingDate, retryToken);

                var requestHash = ComputeRequestHash(request.CourtId, canonicalBucketIds, request.BookingMode, request.SlotQuantity);
                const string endpoint = "/api/holds";
                var idempotencyScope = request.UserId;

                var existingIdempotency = await dbContext.IdempotencyKeys
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        record =>
                            record.Scope == idempotencyScope &&
                            record.Endpoint == endpoint &&
                            record.IdempotencyKeyValue == request.IdempotencyKey,
                        retryToken);

                if (existingIdempotency is not null)
                {
                    if (!string.Equals(existingIdempotency.RequestHash, requestHash, StringComparison.Ordinal))
                    {
                        return new HoldOperationResult(false, null, "Idempotency-Key da duoc dung voi payload khac.");
                    }

                    var replayedHold = JsonSerializer.Deserialize<HoldSummary>(
                        existingIdempotency.ResponseSnapshot,
                        SnapshotSerializerOptions);

                    return new HoldOperationResult(true, replayedHold, Replayed: true);
                }

                var useTransaction = dbContext.Database.IsRelational();
                await using var transaction = useTransaction
                    ? await dbContext.Database.BeginTransactionAsync(retryToken)
                    : null;

                var lockedBuckets = await LockCourtBucketsAsync(request.CourtId, bucketIdList, retryToken);
                if (lockedBuckets.Count != canonicalBucketIds.Length)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(retryToken);
                    }
                    return new HoldOperationResult(false, null, "Khong tim thay day du inventory cho court va khung gio da chon.");
                }

                var timeBuckets = await dbContext.TimeBuckets
                    .Where(bucket => bucketIdList.Contains(bucket.BucketId))
                    .OrderBy(bucket => bucket.StartAt)
                    .ToDictionaryAsync(bucket => bucket.BucketId, retryToken);

                var validationError = ValidateCourtBuckets(lockedBuckets, timeBuckets, request.BookingMode, request.SlotQuantity);
                if (validationError is not null)
                {
                    if (transaction is not null)
                    {
                        await transaction.RollbackAsync(retryToken);
                    }
                    return new HoldOperationResult(false, null, validationError, Conflict: true);
                }

                var court = await dbContext.Courts.SingleAsync(entity => entity.CourtId == request.CourtId, retryToken);
                var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
                var holdId = Guid.NewGuid().ToString();
                var hold = new Hold
                {
                    HoldId = holdId,
                    UserId = request.UserId,
                    Status = HoldStatus.Active,
                    ExpiresAt = nowUtc.AddMinutes(15),
                    CreatedAt = nowUtc,
                    UpdatedAt = nowUtc
                };

                var items = new List<HoldItem>(canonicalBucketIds.Length);
                foreach (var bucketId in canonicalBucketIds)
                {
                    var timeBucket = timeBuckets[bucketId];
                    var quantity = request.BookingMode == BookingMode.Shared ? request.SlotQuantity : (byte)1;
                    var priceQuote = await priceCalculationService.CalculateAsync(
                        new PriceQuoteRequest(timeBucket.StartAt, request.BookingMode, quantity),
                        retryToken);

                    items.Add(new HoldItem
                    {
                        HoldItemId = Guid.NewGuid().ToString(),
                        HoldId = holdId,
                        CourtId = request.CourtId,
                        BucketId = bucketId,
                        BookingMode = request.BookingMode,
                        SlotQty = quantity,
                        UnitPrice = priceQuote.UnitPrice,
                        LineTotal = priceQuote.TotalPrice,
                        CreatedAt = nowUtc
                    });
                }

                foreach (var courtBucket in lockedBuckets)
                {
                    if (request.BookingMode == BookingMode.Private)
                    {
                        courtBucket.Mode = CourtBucketMode.Private;
                        courtBucket.PrivateHoldId = holdId;
                    }
                    else
                    {
                        courtBucket.Mode = CourtBucketMode.Shared;
                        courtBucket.SharedReserved += request.SlotQuantity;
                    }

                    courtBucket.LockVersion += 1;
                    courtBucket.UpdatedAt = nowUtc;
                }

                await dbContext.Holds.AddAsync(hold, retryToken);
                await dbContext.HoldItems.AddRangeAsync(items, retryToken);

                var summary = BuildSummary(hold, items, court, timeBuckets);
                await dbContext.IdempotencyKeys.AddAsync(
                    new IdempotencyKey
                    {
                        IdempotencyId = Guid.NewGuid().ToString(),
                        Scope = idempotencyScope,
                        Endpoint = endpoint,
                        IdempotencyKeyValue = request.IdempotencyKey,
                        RequestHash = requestHash,
                        ResponseStatus = 201,
                        ResponseSnapshot = JsonSerializer.Serialize(summary, SnapshotSerializerOptions),
                        ExpiresAt = hold.ExpiresAt,
                        CreatedAt = nowUtc
                    },
                    retryToken);

                await dbContext.SaveChangesAsync(retryToken);
                if (transaction is not null)
                {
                    await transaction.CommitAsync(retryToken);
                }
                return new HoldOperationResult(true, summary);
            },
            cancellationToken);
    }

    public async Task<HoldSummary?> GetSummaryAsync(string holdId, string userId, CancellationToken cancellationToken = default)
    {
        await holdLifecycleService.ExpireHoldIfNeededAsync(holdId, cancellationToken);

        var hold = await dbContext.Holds
            .AsNoTracking()
            .Include(entity => entity.HoldItems)
                .ThenInclude(item => item.Court)
            .Include(entity => entity.HoldItems)
                .ThenInclude(item => item.TimeBucket)
            .SingleOrDefaultAsync(entity => entity.HoldId == holdId && entity.UserId == userId, cancellationToken);

        if (hold is null)
        {
            return null;
        }

        var items = hold.HoldItems
            .OrderBy(item => item.TimeBucket.StartAt)
            .Select(item => new HoldSummaryItem(
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
            .ToArray();

        return new HoldSummary(
            hold.HoldId,
            hold.UserId,
            hold.Status,
            hold.ExpiresAt,
            hold.CreatedAt,
            items.Sum(item => item.LineTotal),
            items);
    }

    private static void ValidateRequest(CreateHoldRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new InvalidOperationException("Authenticated user id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CourtId))
        {
            throw new InvalidOperationException("Court id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new InvalidOperationException("Idempotency-Key is required.");
        }

        if (request.SlotQuantity == 0)
        {
            throw new InvalidOperationException("Slot quantity must be greater than zero.");
        }

        if (request.BookingMode == BookingMode.Private && request.SlotQuantity != 1)
        {
            throw new InvalidOperationException("Private hold currently supports exactly one court slot per bucket.");
        }
    }

    private async Task<List<CourtBucket>> LockCourtBucketsAsync(string courtId, IReadOnlyList<string> bucketIds, CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            return await dbContext.CourtBuckets
                .Where(bucket => bucket.CourtId == courtId && bucketIds.Contains(bucket.BucketId))
                .OrderBy(bucket => bucket.BucketId)
                .ToListAsync(cancellationToken);
        }

        var placeholders = string.Join(", ", Enumerable.Range(0, bucketIds.Count).Select(index => $"{{{index + 1}}}"));
        var parameters = new object[] { courtId }.Concat(bucketIds.Cast<object>()).ToArray();
        var sql = $"SELECT * FROM court_buckets WHERE court_id = {{0}} AND bucket_id IN ({placeholders}) ORDER BY bucket_id FOR UPDATE";

        return await dbContext.CourtBuckets
            .FromSqlRaw(sql, parameters)
            .ToListAsync(cancellationToken);
    }

    private async Task<DateOnly> ResolveBookingDateAsync(IReadOnlyList<string> bucketIds, CancellationToken cancellationToken)
    {
        var firstBucket = await dbContext.TimeBuckets
            .AsNoTracking()
            .Where(bucket => bucketIds.Contains(bucket.BucketId))
            .OrderBy(bucket => bucket.StartAt)
            .Select(bucket => bucket.StartAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (firstBucket == default)
        {
            throw new InvalidOperationException("Selected booking buckets were not found.");
        }

        return DateOnly.FromDateTime(firstBucket);
    }

    private static string? ValidateCourtBuckets(
        IReadOnlyList<CourtBucket> lockedBuckets,
        IReadOnlyDictionary<string, TimeBucket> timeBuckets,
        BookingMode bookingMode,
        byte slotQuantity)
    {
        if (lockedBuckets.Any(bucket => !timeBuckets.ContainsKey(bucket.BucketId)))
        {
            return "Khong tim thay du lieu time bucket cho cac khung gio da chon.";
        }

        foreach (var bucket in lockedBuckets.OrderBy(item => timeBuckets[item.BucketId].StartAt))
        {
            if (bookingMode == BookingMode.Private)
            {
                if (bucket.Mode != CourtBucketMode.None)
                {
                    return $"Khung gio {timeBuckets[bucket.BucketId].StartAt:HH:mm} khong con trong cho private hold.";
                }

                continue;
            }

            if (bucket.Mode == CourtBucketMode.Private)
            {
                return $"Khung gio {timeBuckets[bucket.BucketId].StartAt:HH:mm} dang duoc giu cho private booking.";
            }

            if (bucket.SharedReserved + slotQuantity > bucket.SharedCapacity)
            {
                return $"Khung gio {timeBuckets[bucket.BucketId].StartAt:HH:mm} khong con du slot shared.";
            }
        }

        return null;
    }

    private static HoldSummary BuildSummary(
        Hold hold,
        IReadOnlyList<HoldItem> items,
        Court court,
        IReadOnlyDictionary<string, TimeBucket> timeBuckets)
    {
        var summaryItems = items
            .OrderBy(item => timeBuckets[item.BucketId].StartAt)
            .Select(item =>
            {
                var timeBucket = timeBuckets[item.BucketId];
                return new HoldSummaryItem(
                    item.HoldItemId,
                    item.CourtId,
                    court.CourtCode,
                    court.CourtName,
                    item.BucketId,
                    timeBucket.StartAt,
                    timeBucket.EndAt,
                    item.BookingMode,
                    item.SlotQty,
                    item.UnitPrice,
                    item.LineTotal);
            })
            .ToArray();

        return new HoldSummary(
            hold.HoldId,
            hold.UserId,
            hold.Status,
            hold.ExpiresAt,
            hold.CreatedAt,
            summaryItems.Sum(item => item.LineTotal),
            summaryItems);
    }

    private static string ComputeRequestHash(
        string courtId,
        IReadOnlyList<string> bucketIds,
        BookingMode bookingMode,
        byte slotQuantity)
    {
        var payload = JsonSerializer.Serialize(new
        {
            courtId,
            bucketIds,
            bookingMode,
            slotQuantity
        });

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
