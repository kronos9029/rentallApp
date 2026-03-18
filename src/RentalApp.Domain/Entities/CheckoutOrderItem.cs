namespace RentalApp.Domain.Entities;

public sealed class CheckoutOrderItem
{
    public string OrderItemId { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public string BookingId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public CheckoutOrder Order { get; set; } = null!;

    public Booking Booking { get; set; } = null!;
}
