using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace RentalApp.Infrastructure.Booking;

public sealed class DeadlockRetryExecutor
{
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception exception) when (attempt < maxAttempts && IsTransientMySqlLockFailure(exception))
            {
                var delayMs = Random.Shared.Next(40, 120) * attempt;
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken);
            }
        }
    }

    private static bool IsTransientMySqlLockFailure(Exception exception)
    {
        return exception switch
        {
            MySqlException { Number: 1205 or 1213 } => true,
            DbUpdateException { InnerException: MySqlException { Number: 1205 or 1213 } } => true,
            _ => false
        };
    }
}
