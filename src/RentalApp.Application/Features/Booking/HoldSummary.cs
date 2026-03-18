using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Booking;

public sealed record HoldSummary(
    string HoldId,
    string UserId,
    HoldStatus Status,
    DateTime ExpiresAtUtc,
    DateTime CreatedAtUtc,
    decimal TotalAmount,
    IReadOnlyList<HoldSummaryItem> Items);
