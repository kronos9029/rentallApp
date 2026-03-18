using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Booking;

public sealed record CheckoutOrderSummary(
    string OrderId,
    string BookingId,
    string HoldId,
    CheckoutOrderStatus Status,
    decimal TotalAmount,
    string Currency,
    DateTime? ExpiresAtUtc,
    DateTime CreatedAtUtc,
    IReadOnlyList<CheckoutItemSummary> Items);
