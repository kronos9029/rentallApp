namespace RentalApp.Domain.Entities;

public sealed class PasswordResetToken
{
    public string TokenId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
