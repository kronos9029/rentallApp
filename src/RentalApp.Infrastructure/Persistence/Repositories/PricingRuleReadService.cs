using Microsoft.EntityFrameworkCore;
using RentalApp.Application.Features.Pricing;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Domain.Services;

namespace RentalApp.Infrastructure.Persistence.Repositories;

public sealed class PricingRuleReadService(RentalAppDbContext dbContext) : IPriceCalculationService
{
    public async Task<PriceQuote> CalculateAsync(PriceQuoteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        var candidates = await dbContext.PricingRules
            .AsNoTracking()
            .Where(rule =>
                rule.IsActive &&
                rule.EffectiveFrom <= request.StartAtUtc &&
                (rule.EffectiveTo == null || rule.EffectiveTo > request.StartAtUtc))
            .ToListAsync(cancellationToken);

        var isWeekend = request.StartAtUtc.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var bookingTime = TimeOnly.FromDateTime(request.StartAtUtc);

        var rule = candidates
            .Where(candidate => MatchesDayType(candidate, isWeekend))
            .Where(candidate => MatchesBookingMode(candidate, request.BookingMode))
            .Where(candidate => MatchesTimeWindow(candidate, bookingTime))
            .OrderByDescending(candidate => candidate.AppliesTo == PricingAppliesTo.Any ? 0 : 1)
            .ThenByDescending(candidate => candidate.DayType == PricingDayType.Any ? 0 : 1)
            .ThenByDescending(candidate => candidate.EffectiveFrom)
            .FirstOrDefault()
            ?? candidates
                .Where(candidate => MatchesDayType(candidate, isWeekend))
                .Where(candidate => MatchesBookingMode(candidate, request.BookingMode))
                .OrderByDescending(candidate => candidate.AppliesTo == PricingAppliesTo.Any ? 0 : 1)
                .ThenByDescending(candidate => candidate.DayType == PricingDayType.Any ? 0 : 1)
                .ThenByDescending(candidate => candidate.EffectiveFrom)
                .FirstOrDefault();

        if (rule is null)
        {
            throw new InvalidOperationException("No active pricing rule matched the requested booking window.");
        }

        var unitPrice = PricingCalculator.CalculateUnitPrice(rule, request.BookingMode, isWeekend);
        return new PriceQuote(
            rule.PricingRuleId,
            rule.RuleName,
            unitPrice,
            unitPrice * request.Quantity,
            isWeekend);
    }

    private static bool MatchesDayType(PricingRule rule, bool isWeekend)
    {
        return rule.DayType switch
        {
            PricingDayType.Any => true,
            PricingDayType.Weekday => !isWeekend,
            PricingDayType.Weekend => isWeekend,
            _ => false
        };
    }

    private static bool MatchesBookingMode(PricingRule rule, BookingMode bookingMode)
    {
        return rule.AppliesTo switch
        {
            PricingAppliesTo.Any => true,
            PricingAppliesTo.Private => bookingMode == BookingMode.Private,
            PricingAppliesTo.Shared => bookingMode == BookingMode.Shared,
            _ => false
        };
    }

    private static bool MatchesTimeWindow(PricingRule rule, TimeOnly bookingTime)
    {
        if (rule.StartTime is null && rule.EndTime is null)
        {
            return true;
        }

        if (rule.StartTime is not null && bookingTime < rule.StartTime.Value)
        {
            return false;
        }

        if (rule.EndTime is not null && bookingTime >= rule.EndTime.Value)
        {
            return false;
        }

        return true;
    }
}
