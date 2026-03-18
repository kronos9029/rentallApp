namespace RentalApp.Application.Features.Availability;

public interface IAvailabilityService
{
    Task<AvailabilityResult> GetAvailabilityAsync(AvailabilityQuery query, CancellationToken cancellationToken = default);
}
