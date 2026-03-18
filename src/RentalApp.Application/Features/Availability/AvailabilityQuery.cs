using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Availability;

public sealed record AvailabilityQuery(
    DateOnly BookingDate,
    BookingMode BookingMode,
    byte SlotQuantity = 1);
