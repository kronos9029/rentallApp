using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Availability;

public sealed record AvailabilityResult(
    DateOnly BookingDate,
    BookingMode BookingMode,
    byte SlotQuantity,
    IReadOnlyList<AvailabilityCourtOption> Courts);
