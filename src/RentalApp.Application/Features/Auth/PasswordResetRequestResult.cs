namespace RentalApp.Application.Features.Auth;

public sealed record PasswordResetRequestResult(
    bool UserExists,
    string? Token,
    DateTimeOffset? ExpiresAt);
