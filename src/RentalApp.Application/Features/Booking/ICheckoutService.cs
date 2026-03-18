namespace RentalApp.Application.Features.Booking;

public interface ICheckoutService
{
    Task<CheckoutPreview?> GetPreviewAsync(string holdId, string userId, CancellationToken cancellationToken = default);

    Task<CheckoutOperationResult> CreateAsync(CreateCheckoutRequest request, CancellationToken cancellationToken = default);
}
