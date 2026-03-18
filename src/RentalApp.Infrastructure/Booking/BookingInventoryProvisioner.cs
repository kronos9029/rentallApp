using Microsoft.EntityFrameworkCore;
using RentalApp.Domain.Entities;
using RentalApp.Infrastructure.Persistence;

namespace RentalApp.Infrastructure.Booking;

public sealed class BookingInventoryProvisioner(RentalAppDbContext dbContext, TimeProvider timeProvider)
{
    private const int OpeningHour = 6;
    private const int ClosingHour = 22;
    private const int SlotDurationMinutes = 60;

    public async Task EnsureDailyInventoryAsync(DateOnly bookingDate, CancellationToken cancellationToken = default)
    {
        var windows = BuildDailyWindows(bookingDate);
        var rangeStart = windows[0].StartAt;
        var rangeEnd = windows[^1].EndAt;

        var existingBuckets = await dbContext.TimeBuckets
            .Where(bucket => bucket.StartAt >= rangeStart && bucket.EndAt <= rangeEnd)
            .ToListAsync(cancellationToken);

        var bucketLookup = existingBuckets.ToDictionary(
            bucket => $"{bucket.StartAt:O}|{bucket.EndAt:O}",
            bucket => bucket);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var missingBuckets = windows
            .Where(window => !bucketLookup.ContainsKey(window.Key))
            .Select(window => new TimeBucket
            {
                BucketId = Guid.NewGuid().ToString(),
                StartAt = window.StartAt,
                EndAt = window.EndAt,
                DurationMin = SlotDurationMinutes,
                CreatedAt = nowUtc
            })
            .ToArray();

        if (missingBuckets.Length > 0)
        {
            if (dbContext.Database.IsRelational())
            {
                foreach (var bucket in missingBuckets)
                {
                    await dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $"""
                        INSERT IGNORE INTO time_buckets (bucket_id, created_at, duration_min, end_at, start_at)
                        VALUES ({bucket.BucketId}, {bucket.CreatedAt}, {bucket.DurationMin}, {bucket.EndAt}, {bucket.StartAt})
                        """,
                        cancellationToken);
                }
            }
            else
            {
                await dbContext.TimeBuckets.AddRangeAsync(missingBuckets, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var buckets = await dbContext.TimeBuckets
            .Where(bucket => bucket.StartAt >= rangeStart && bucket.EndAt <= rangeEnd)
            .ToListAsync(cancellationToken);

        var activeCourts = await dbContext.Courts
            .Where(court => court.IsActive)
            .OrderBy(court => court.SortOrder)
            .ToListAsync(cancellationToken);

        var bucketIds = buckets.Select(bucket => bucket.BucketId).ToList();
        var existingCourtBuckets = await dbContext.CourtBuckets
            .Where(bucket => bucketIds.Contains(bucket.BucketId))
            .Select(bucket => new { bucket.CourtId, bucket.BucketId })
            .ToListAsync(cancellationToken);

        var existingCourtBucketSet = existingCourtBuckets
            .Select(item => $"{item.CourtId}|{item.BucketId}")
            .ToHashSet(StringComparer.Ordinal);

        var missingCourtBuckets = activeCourts
            .SelectMany(court => buckets.Select(bucket => new { court, bucket }))
            .Where(item => !existingCourtBucketSet.Contains($"{item.court.CourtId}|{item.bucket.BucketId}"))
            .Select(item => new CourtBucket
            {
                CourtId = item.court.CourtId,
                BucketId = item.bucket.BucketId,
                UpdatedAt = nowUtc
            })
            .ToArray();

        if (missingCourtBuckets.Length == 0)
        {
            return;
        }

        if (dbContext.Database.IsRelational())
        {
            foreach (var courtBucket in missingCourtBuckets)
            {
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                    INSERT IGNORE INTO court_buckets (court_id, bucket_id, updated_at)
                    VALUES ({courtBucket.CourtId}, {courtBucket.BucketId}, {courtBucket.UpdatedAt})
                    """,
                    cancellationToken);
            }
        }
        else
        {
            await dbContext.CourtBuckets.AddRangeAsync(missingCourtBuckets, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static IReadOnlyList<BucketWindow> BuildDailyWindows(DateOnly bookingDate)
    {
        var startAt = new DateTime(
            bookingDate.Year,
            bookingDate.Month,
            bookingDate.Day,
            OpeningHour,
            0,
            0,
            DateTimeKind.Utc);

        return Enumerable.Range(0, ClosingHour - OpeningHour)
            .Select(index =>
            {
                var slotStart = startAt.AddHours(index);
                var slotEnd = slotStart.AddMinutes(SlotDurationMinutes);
                return new BucketWindow($"{slotStart:O}|{slotEnd:O}", slotStart, slotEnd);
            })
            .ToArray();
    }

    private sealed record BucketWindow(string Key, DateTime StartAt, DateTime EndAt);
}
