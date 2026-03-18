namespace RentalApp.Application.Features.Booking;

public interface IHoldLifecycleService
{
    Task<int> ExpireExpiredHoldsAsync(CancellationToken cancellationToken = default);

    Task<bool> ExpireHoldIfNeededAsync(string holdId, CancellationToken cancellationToken = default);
}
