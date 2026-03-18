namespace RentalApp.Application.Features.Auth;

public sealed record AuthenticatedUserResult(
    bool Success,
    string? Error,
    AuthenticatedUser? User);
