using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Booking;

public sealed record CheckoutPreview(
    string HoldId,
    HoldStatus HoldStatus,
    DateTime ExpiresAtUtc,
    decimal TotalAmount,
    bool CanCheckout,
    string? ExistingOrderId,
    IReadOnlyList<CheckoutItemSummary> Items,
    string? ErrorMessage = null);
