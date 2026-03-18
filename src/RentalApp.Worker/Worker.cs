using RentalApp.Application.Features.Booking;

namespace RentalApp.Worker;

public sealed class Worker(
    IServiceScopeFactory scopeFactory,
    ILogger<Worker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var holdLifecycleService = scope.ServiceProvider.GetRequiredService<IHoldLifecycleService>();
                var expiredCount = await holdLifecycleService.ExpireExpiredHoldsAsync(stoppingToken);

                logger.LogInformation("Hold expiry worker tick complete. Expired holds: {expiredCount}", expiredCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Hold expiry worker failed.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
