using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class BookingItem
{
    public string BookingItemId { get; set; } = string.Empty;

    public string BookingId { get; set; } = string.Empty;

    public string CourtId { get; set; } = string.Empty;

    public string BucketId { get; set; } = string.Empty;

    public BookingMode BookingMode { get; set; }

    public byte SlotQty { get; set; } = 1;

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public BookingItemStatus Status { get; set; } = BookingItemStatus.Active;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;

    public Court Court { get; set; } = null!;

    public TimeBucket TimeBucket { get; set; } = null!;
}
