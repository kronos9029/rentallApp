namespace RentalApp.Application.Features.Booking;

public interface IHoldService
{
    Task<HoldOperationResult> CreateAsync(CreateHoldRequest request, CancellationToken cancellationToken = default);

    Task<HoldSummary?> GetSummaryAsync(string holdId, string userId, CancellationToken cancellationToken = default);
}
