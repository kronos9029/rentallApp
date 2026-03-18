namespace RentalApp.Application.Features.Booking;

public sealed record HoldOperationResult(
    bool Success,
    HoldSummary? Hold,
    string? Error = null,
    bool Conflict = false,
    bool Replayed = false);
