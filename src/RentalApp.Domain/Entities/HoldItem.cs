using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class HoldItem
{
    public string HoldItemId { get; set; } = string.Empty;

    public string HoldId { get; set; } = string.Empty;

    public string CourtId { get; set; } = string.Empty;

    public string BucketId { get; set; } = string.Empty;

    public BookingMode BookingMode { get; set; }

    public byte SlotQty { get; set; } = 1;

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public Hold Hold { get; set; } = null!;

    public Court Court { get; set; } = null!;

    public TimeBucket TimeBucket { get; set; } = null!;
}
