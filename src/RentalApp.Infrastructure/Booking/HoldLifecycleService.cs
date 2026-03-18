using Microsoft.EntityFrameworkCore;
using RentalApp.Application.Features.Booking;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;

namespace RentalApp.Infrastructure.Booking;

public sealed class HoldLifecycleService(
    RentalAppDbContext dbContext,
    DeadlockRetryExecutor retryExecutor,
    TimeProvider timeProvider) : IHoldLifecycleService
{
    private const int DefaultBatchSize = 25;

    public async Task<int> ExpireExpiredHoldsAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var expiredIds = await dbContext.Holds
            .AsNoTracking()
            .Where(hold => hold.Status == HoldStatus.Active && hold.ExpiresAt <= nowUtc)
            .OrderBy(hold => hold.ExpiresAt)
            .Select(hold => hold.HoldId)
            .Take(DefaultBatchSize)
            .ToArrayAsync(cancellationToken);

        var expiredCount = 0;
        foreach (var holdId in expiredIds)
        {
            if (await ExpireHoldIfNeededAsync(holdId, cancellationToken))
            {
                expiredCount++;
            }
        }

        return expiredCount;
    }

    public Task<bool> ExpireHoldIfNeededAsync(string holdId, CancellationToken cancellationToken = default)
    {
        return retryExecutor.ExecuteAsync(async retryToken =>
        {
            var useTransaction = dbContext.Database.IsRelational();
            await using var transaction = useTransaction
                ? await dbContext.Database.BeginTransactionAsync(retryToken)
                : null;

            var hold = useTransaction
                ? await dbContext.Holds
                    .FromSqlInterpolated($"SELECT * FROM holds WHERE hold_id = {holdId} FOR UPDATE")
                    .Include(entity => entity.HoldItems)
                    .SingleOrDefaultAsync(retryToken)
                : await dbContext.Holds
                    .Include(entity => entity.HoldItems)
                    .SingleOrDefaultAsync(entity => entity.HoldId == holdId, retryToken);

            if (hold is null || hold.Status != HoldStatus.Active)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(retryToken);
                }
                return false;
            }

            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            if (hold.ExpiresAt > nowUtc)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(retryToken);
                }
                return false;
            }

            var courtGroups = hold.HoldItems
                .GroupBy(item => item.CourtId)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.BucketId).ToArray(), StringComparer.Ordinal);

            foreach (var group in courtGroups.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                var bucketIds = group.Value.Select(item => item.BucketId).Distinct(StringComparer.Ordinal).OrderBy(bucketId => bucketId, StringComparer.Ordinal).ToArray();
                var lockedBuckets = await LockCourtBucketsAsync(group.Key, bucketIds, retryToken);

                foreach (var holdItem in group.Value)
                {
                    var bucket = lockedBuckets.Single(entity => entity.BucketId == holdItem.BucketId);
                    ReleaseInventory(bucket, hold, holdItem, nowUtc);
                }
            }

            hold.Status = HoldStatus.Expired;
            hold.UpdatedAt = nowUtc;

            await dbContext.SaveChangesAsync(retryToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(retryToken);
            }
            return true;
        }, cancellationToken);
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

    private static void ReleaseInventory(CourtBucket bucket, Hold hold, HoldItem holdItem, DateTime nowUtc)
    {
        if (holdItem.BookingMode == BookingMode.Private)
        {
            if (string.Equals(bucket.PrivateHoldId, hold.HoldId, StringComparison.Ordinal))
            {
                bucket.PrivateHoldId = null;
                bucket.Mode = CourtBucketMode.None;
            }
        }
        else
        {
            bucket.SharedReserved = (byte)Math.Max(0, bucket.SharedReserved - holdItem.SlotQty);
            bucket.Mode = bucket.SharedReserved == 0 ? CourtBucketMode.None : CourtBucketMode.Shared;
        }

        bucket.LockVersion += 1;
        bucket.UpdatedAt = nowUtc;
    }
}
