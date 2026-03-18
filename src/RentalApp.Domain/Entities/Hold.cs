using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class Hold
{
    public string HoldId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public HoldStatus Status { get; set; } = HoldStatus.Active;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public AppUser User { get; set; } = null!;

    public ICollection<HoldItem> HoldItems { get; set; } = [];
}
