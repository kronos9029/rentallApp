using RentalApp.Domain.Enums;

namespace RentalApp.Application.Features.Pricing;

public sealed record PriceQuoteRequest(
    DateTime StartAtUtc,
    BookingMode BookingMode,
    int Quantity = 1);
