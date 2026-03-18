namespace RentalApp.Application.Features.Availability;

public sealed record AvailabilityCourtOption(
    string CourtId,
    string CourtCode,
    string CourtName,
    IReadOnlyList<AvailabilitySlotOption> Slots);
