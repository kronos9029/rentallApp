using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class Booking
{
    public string BookingId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string? HoldId { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalAmount { get; set; }

    public DateOnly BookingDate { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? CancellationPolicyId { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public AppUser User { get; set; } = null!;

    public Hold? Hold { get; set; }

    public CancellationPolicy? CancellationPolicy { get; set; }

    public ICollection<BookingItem> BookingItems { get; set; } = [];

    public ICollection<CheckoutOrderItem> CheckoutOrderItems { get; set; } = [];
}
