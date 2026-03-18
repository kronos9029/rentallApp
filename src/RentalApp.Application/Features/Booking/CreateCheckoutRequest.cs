namespace RentalApp.Application.Features.Booking;

public sealed record CreateCheckoutRequest(
    string UserId,
    string HoldId,
    string IdempotencyKey);
