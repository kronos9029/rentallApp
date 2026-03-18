namespace RentalApp.Application.Features.Availability;

public sealed record AvailabilitySlotOption(
    string BucketId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    bool IsAvailable,
    byte RemainingSharedSlots,
    decimal UnitPrice,
    decimal TotalPrice,
    string? UnavailableReason);
