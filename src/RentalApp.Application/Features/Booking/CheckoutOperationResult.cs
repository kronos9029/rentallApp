namespace RentalApp.Application.Features.Booking;

public sealed record CheckoutOperationResult(
    bool Success,
    CheckoutOrderSummary? Order,
    string? Error = null,
    bool Conflict = false,
    bool Replayed = false);
