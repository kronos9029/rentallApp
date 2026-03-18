using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalApp.Domain.Entities;
using RentalApp.Infrastructure.Persistence.Seed;

namespace RentalApp.Infrastructure.Persistence.Configurations;

internal sealed class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("pricing_rules");
        builder.HasKey(rule => rule.PricingRuleId);
        builder.Property(rule => rule.PricingRuleId).HasColumnName("pricing_rule_id").HasMaxLength(36);
        builder.Property(rule => rule.RuleName).HasColumnName("rule_name").HasMaxLength(120).IsRequired();
        builder.Property(rule => rule.AppliesTo).HasColumnName("applies_to").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(rule => rule.DayType).HasColumnName("day_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(rule => rule.StartTime).HasColumnName("start_time");
        builder.Property(rule => rule.EndTime).HasColumnName("end_time");
        builder.Property(rule => rule.PrivateRate).HasColumnName("private_rate").HasPrecision(12, 2);
        builder.Property(rule => rule.SharedRate).HasColumnName("shared_rate").HasPrecision(12, 2);
        builder.Property(rule => rule.WeekendMarkupPct).HasColumnName("weekend_markup_pct").HasPrecision(5, 2);
        builder.Property(rule => rule.IsActive).HasColumnName("is_active");
        builder.Property(rule => rule.EffectiveFrom).HasColumnName("effective_from").HasPrecision(6);
        builder.Property(rule => rule.EffectiveTo).HasColumnName("effective_to").HasPrecision(6);
        builder.Property(rule => rule.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(36).IsRequired();
        builder.Property(rule => rule.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(rule => rule.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(rule => new { rule.IsActive, rule.EffectiveFrom });
        builder.HasOne(rule => rule.CreatedByUser)
            .WithMany(user => user.CreatedPricingRules)
            .HasForeignKey(rule => rule.CreatedByUserId);
        builder.HasData(AppSeedData.PricingRules);
    }
}

internal sealed class CancellationPolicyConfiguration : IEntityTypeConfiguration<CancellationPolicy>
{
    public void Configure(EntityTypeBuilder<CancellationPolicy> builder)
    {
        builder.ToTable("cancellation_policies");
        builder.HasKey(policy => policy.PolicyId);
        builder.Property(policy => policy.PolicyId).HasColumnName("policy_id").HasMaxLength(36);
        builder.Property(policy => policy.PolicyName).HasColumnName("policy_name").HasMaxLength(100).IsRequired();
        builder.Property(policy => policy.MinHoursBeforeStart).HasColumnName("min_hours_before_start");
        builder.Property(policy => policy.RefundStrategy).HasColumnName("refund_strategy").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(policy => policy.RefundPercent).HasColumnName("refund_percent").HasPrecision(5, 2);
        builder.Property(policy => policy.CancelFeePercent).HasColumnName("cancel_fee_percent").HasPrecision(5, 2);
        builder.Property(policy => policy.IsActive).HasColumnName("is_active");
        builder.Property(policy => policy.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(policy => policy.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(policy => policy.IsActive);
        builder.HasData(AppSeedData.StandardCancellationPolicy);
    }
}
