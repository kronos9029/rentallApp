using RentalApp.Domain.Common;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;

namespace RentalApp.Infrastructure.Persistence.Seed;

public static class AppSeedData
{
    public const string AdminRoleId = "00000000-0000-0000-0000-000000000001";
    public const string CustomerRoleId = "00000000-0000-0000-0000-000000000002";
    public const string SeedAdminUserId = "10000000-0000-0000-0000-000000000001";
    public const string SeedCustomerUserId = "10000000-0000-0000-0000-000000000002";
    public const string StandardCancellationPolicyId = "30000000-0000-0000-0000-000000000001";
    public const string WeekdayPricingRuleId = "40000000-0000-0000-0000-000000000001";
    public const string WeekendPricingRuleId = "40000000-0000-0000-0000-000000000002";
    public const string SeedAdminPasswordHash = "AQAAAAIAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v+9B/m68fN7pVomQZIk2QhdaT58yv9z9zo2k0tHeC3mUA==";
    public const string SeedCustomerPasswordHash = "AQAAAAIAAYagAAAAEBAhMkNUZXaHmKm6y9zt/g/dvnXaKV7KW02eQ7Sj/tJyjyBsxoyog7jpqOJeHhupEg==";

    public static readonly DateTime SeedTimestampUtc = new(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime PricingEffectiveFromUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly IReadOnlyList<Court> Courts = CreateCourts();

    public static IReadOnlyList<Role> Roles =>
    [
        new()
        {
            RoleId = AdminRoleId,
            RoleName = AppRoles.Admin,
            CreatedAt = SeedTimestampUtc
        },
        new()
        {
            RoleId = CustomerRoleId,
            RoleName = AppRoles.Customer,
            CreatedAt = SeedTimestampUtc
        }
    ];

    public static AppUser SeedAdminUser =>
        new()
        {
            UserId = SeedAdminUserId,
            Email = "admin@local.test",
            Username = "admin.local",
            PasswordHash = SeedAdminPasswordHash,
            Status = UserStatus.Active,
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        };

    public static AppUser SeedCustomerUser =>
        new()
        {
            UserId = SeedCustomerUserId,
            Email = "customer@local.test",
            Username = "customer.local",
            PasswordHash = SeedCustomerPasswordHash,
            Status = UserStatus.Active,
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        };

    public static UserProfile SeedAdminProfile =>
        new()
        {
            UserId = SeedAdminUserId,
            FullName = "Admin Local",
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        };

    public static UserProfile SeedCustomerProfile =>
        new()
        {
            UserId = SeedCustomerUserId,
            FullName = "Customer Local",
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        };

    public static UserRole SeedAdminUserRole =>
        new()
        {
            UserId = SeedAdminUserId,
            RoleId = AdminRoleId,
            AssignedAt = SeedTimestampUtc
        };

    public static UserRole SeedCustomerUserRole =>
        new()
        {
            UserId = SeedCustomerUserId,
            RoleId = CustomerRoleId,
            AssignedAt = SeedTimestampUtc
        };

    public static IReadOnlyList<PricingRule> PricingRules =>
    [
        new()
        {
            PricingRuleId = WeekdayPricingRuleId,
            RuleName = "Weekday Baseline",
            AppliesTo = PricingAppliesTo.Any,
            DayType = PricingDayType.Weekday,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(22, 0),
            PrivateRate = 280000m,
            SharedRate = 45000m,
            WeekendMarkupPct = 0m,
            IsActive = true,
            EffectiveFrom = PricingEffectiveFromUtc,
            CreatedByUserId = SeedAdminUserId,
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        },
        new()
        {
            PricingRuleId = WeekendPricingRuleId,
            RuleName = "Weekend Baseline",
            AppliesTo = PricingAppliesTo.Any,
            DayType = PricingDayType.Weekend,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(22, 0),
            PrivateRate = 280000m,
            SharedRate = 45000m,
            WeekendMarkupPct = 20m,
            IsActive = true,
            EffectiveFrom = PricingEffectiveFromUtc,
            CreatedByUserId = SeedAdminUserId,
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        }
    ];

    public static CancellationPolicy StandardCancellationPolicy =>
        new()
        {
            PolicyId = StandardCancellationPolicyId,
            PolicyName = "Standard 2 Hour Refund",
            MinHoursBeforeStart = 2,
            RefundStrategy = RefundStrategy.Full,
            RefundPercent = 100m,
            CancelFeePercent = 0m,
            IsActive = true,
            CreatedAt = SeedTimestampUtc,
            UpdatedAt = SeedTimestampUtc
        };

    private static IReadOnlyList<Court> CreateCourts()
    {
        return Enumerable.Range(1, 9)
            .Select(index => new Court
            {
                CourtId = $"20000000-0000-0000-0000-00000000000{index}",
                CourtCode = $"C{index:00}",
                CourtName = $"Court {index:00}",
                SortOrder = (byte)index,
                IsActive = true,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc
            })
            .ToArray();
    }
}
