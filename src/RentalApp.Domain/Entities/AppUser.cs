using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class AppUser
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserProfile? Profile { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];

    public ICollection<PricingRule> CreatedPricingRules { get; set; } = [];

    public ICollection<Hold> Holds { get; set; } = [];

    public ICollection<Booking> Bookings { get; set; } = [];

    public ICollection<CheckoutOrder> CheckoutOrders { get; set; } = [];
}
