using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Booking;

public sealed record CreateHoldRequest(
    string UserId,
    string CourtId,
    IReadOnlyList<string> BucketIds,
    BookingMode BookingMode,
    byte SlotQuantity,
    string IdempotencyKey);
