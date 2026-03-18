using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class PricingRule
{
    public string PricingRuleId { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public PricingAppliesTo AppliesTo { get; set; } = PricingAppliesTo.Any;

    public PricingDayType DayType { get; set; } = PricingDayType.Any;

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public decimal PrivateRate { get; set; }

    public decimal SharedRate { get; set; }

    public decimal WeekendMarkupPct { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public AppUser CreatedByUser { get; set; } = null!;
}
