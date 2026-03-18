using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Booking;

public sealed record CheckoutItemSummary(
    string BookingItemId,
    string CourtId,
    string CourtCode,
    string CourtName,
    string BucketId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    BookingMode BookingMode,
    byte SlotQuantity,
    decimal UnitPrice,
    decimal LineTotal);
