namespace RentalApp.Application.Features.Auth;

public interface IUserAuthService
{
    Task<AuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<OperationResult> RegisterCustomerAsync(
        string email,
        string fullName,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetProfileAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUserResult> UpdateProfileAsync(
        string userId,
        string fullName,
        CancellationToken cancellationToken = default);

    Task<PasswordResetRequestResult> CreatePasswordResetRequestAsync(
        string email,
        TimeSpan? lifetime = null,
        CancellationToken cancellationToken = default);

    Task<bool> CanResetPasswordAsync(
        string email,
        string token,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}
