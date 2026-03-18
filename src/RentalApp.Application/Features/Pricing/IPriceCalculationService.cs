namespace RentalApp.Application.Features.Pricing;

public interface IPriceCalculationService
{
    Task<PriceQuote> CalculateAsync(PriceQuoteRequest request, CancellationToken cancellationToken = default);
}
