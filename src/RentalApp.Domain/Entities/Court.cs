namespace RentalApp.Domain.Entities;

public sealed class Court
{
    public string CourtId { get; set; } = string.Empty;

    public string CourtCode { get; set; } = string.Empty;

    public string CourtName { get; set; } = string.Empty;

    public byte SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string? MaintenanceReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<CourtBucket> CourtBuckets { get; set; } = [];

    public ICollection<HoldItem> HoldItems { get; set; } = [];

    public ICollection<BookingItem> BookingItems { get; set; } = [];
}
