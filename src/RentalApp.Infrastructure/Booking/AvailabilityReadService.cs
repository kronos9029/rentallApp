using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentalApp.Application.Features.Availability;
using RentalApp.Application.Features.Pricing;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;

namespace RentalApp.Infrastructure.Booking;

public sealed class AvailabilityReadService(
    RentalAppDbContext dbContext,
    BookingInventoryProvisioner inventoryProvisioner,
    IPriceCalculationService priceCalculationService,
    IMemoryCache memoryCache) : IAvailabilityService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<AvailabilityResult> GetAvailabilityAsync(AvailabilityQuery query, CancellationToken cancellationToken = default)
    {
        ValidateQuery(query);

        var cacheKey = $"availability:{query.BookingDate:yyyyMMdd}:{query.BookingMode}:{query.SlotQuantity}";
        if (memoryCache.TryGetValue(cacheKey, out AvailabilityResult? cachedResult) && cachedResult is not null)
        {
            return cachedResult;
        }

        await inventoryProvisioner.EnsureDailyInventoryAsync(query.BookingDate, cancellationToken);

        var dayStartUtc = new DateTime(query.BookingDate.Year, query.BookingDate.Month, query.BookingDate.Day, 0, 0, 0, DateTimeKind.Utc);
        var dayEndUtc = dayStartUtc.AddDays(1);

        var courtBuckets = await dbContext.CourtBuckets
            .AsNoTracking()
            .Include(bucket => bucket.Court)
            .Include(bucket => bucket.TimeBucket)
            .Where(bucket =>
                bucket.Court.IsActive &&
                bucket.TimeBucket.StartAt >= dayStartUtc &&
                bucket.TimeBucket.StartAt < dayEndUtc)
            .OrderBy(bucket => bucket.Court.SortOrder)
            .ThenBy(bucket => bucket.TimeBucket.StartAt)
            .ToListAsync(cancellationToken);

        var courts = new List<AvailabilityCourtOption>();
        foreach (var courtGroup in courtBuckets.GroupBy(bucket => new { bucket.CourtId, bucket.Court.CourtCode, bucket.Court.CourtName }))
        {
            var slots = new List<AvailabilitySlotOption>();
            foreach (var bucket in courtGroup.OrderBy(item => item.TimeBucket.StartAt))
            {
                var (isAvailable, remainingSharedSlots, unavailableReason) = EvaluateAvailability(bucket, query);
                var priceQuote = await priceCalculationService.CalculateAsync(
                    new PriceQuoteRequest(
                        bucket.TimeBucket.StartAt,
                        query.BookingMode,
                        query.BookingMode == BookingMode.Shared ? query.SlotQuantity : 1),
                    cancellationToken);

                slots.Add(new AvailabilitySlotOption(
                    bucket.BucketId,
                    bucket.TimeBucket.StartAt,
                    bucket.TimeBucket.EndAt,
                    isAvailable,
                    remainingSharedSlots,
                    priceQuote.UnitPrice,
                    priceQuote.TotalPrice,
                    unavailableReason));
            }

            courts.Add(new AvailabilityCourtOption(
                courtGroup.Key.CourtId,
                courtGroup.Key.CourtCode,
                courtGroup.Key.CourtName,
                slots));
        }

        var result = new AvailabilityResult(query.BookingDate, query.BookingMode, query.SlotQuantity, courts);
        memoryCache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    private static void ValidateQuery(AvailabilityQuery query)
    {
        if (query.SlotQuantity == 0)
        {
            throw new InvalidOperationException("Slot quantity must be greater than zero.");
        }

        if (query.BookingMode == BookingMode.Private && query.SlotQuantity != 1)
        {
            throw new InvalidOperationException("Private booking currently supports exactly one court slot per bucket.");
        }
    }

    private static (bool IsAvailable, byte RemainingSharedSlots, string? UnavailableReason) EvaluateAvailability(
        CourtBucket bucket,
        AvailabilityQuery query)
    {
        return query.BookingMode switch
        {
            BookingMode.Private when bucket.Mode != CourtBucketMode.None =>
                (false, 0, "Khung gio nay da duoc giu cho booking khac."),
            BookingMode.Private =>
                (true, bucket.SharedCapacity, null),
            BookingMode.Shared when bucket.Mode == CourtBucketMode.Private =>
                (false, 0, "Khung gio nay dang duoc giu private."),
            BookingMode.Shared when bucket.SharedReserved + query.SlotQuantity > bucket.SharedCapacity =>
                (false, (byte)Math.Max(0, bucket.SharedCapacity - bucket.SharedReserved), "Khong con du slot shared."),
            BookingMode.Shared =>
                (true, (byte)(bucket.SharedCapacity - bucket.SharedReserved), null),
            _ => (false, 0, "Khung gio khong hop le.")
        };
    }
}
