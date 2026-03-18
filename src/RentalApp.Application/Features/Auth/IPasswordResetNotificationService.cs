namespace RentalApp.Application.Features.Auth;

public interface IPasswordResetNotificationService
{
    Task<OperationResult> SendResetLinkAsync(
        string email,
        string resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);
}
