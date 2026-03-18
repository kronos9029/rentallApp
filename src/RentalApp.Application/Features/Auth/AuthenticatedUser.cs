namespace RentalApp.Application.Features.Auth;

public sealed record AuthenticatedUser(
    string UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);
