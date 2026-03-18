namespace RentalApp.Application.Features.Auth;

public sealed record AuthenticationResult(
    bool Success,
    string? Error,
    AuthenticatedUser? User);
