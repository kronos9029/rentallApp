namespace RentalApp.Application.Features.Pricing;

public sealed record PriceQuote(
    string PricingRuleId,
    string RuleName,
    decimal UnitPrice,
    decimal TotalPrice,
    bool IsWeekend);
