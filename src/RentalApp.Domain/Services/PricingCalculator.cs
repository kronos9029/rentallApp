using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Services;

public static class PricingCalculator
{
    public static decimal CalculateUnitPrice(PricingRule pricingRule, BookingMode bookingMode, bool isWeekend)
    {
        var baseRate = bookingMode switch
        {
            BookingMode.Private => pricingRule.PrivateRate,
            BookingMode.Shared => pricingRule.SharedRate,
            _ => throw new InvalidOperationException($"Unsupported booking mode: {bookingMode}.")
        };

        if (!isWeekend || pricingRule.WeekendMarkupPct <= 0)
        {
            return decimal.Round(baseRate, 2, MidpointRounding.AwayFromZero);
        }

        var multiplier = 1m + (pricingRule.WeekendMarkupPct / 100m);
        return decimal.Round(baseRate * multiplier, 2, MidpointRounding.AwayFromZero);
    }
}
