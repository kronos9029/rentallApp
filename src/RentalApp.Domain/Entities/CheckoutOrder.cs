using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class CheckoutOrder
{
    public string OrderId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public CheckoutOrderStatus Status { get; set; } = CheckoutOrderStatus.Pending;

    public decimal SubtotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "VND";

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public AppUser User { get; set; } = null!;

    public ICollection<CheckoutOrderItem> OrderItems { get; set; } = [];
}
