namespace RentalApp.Application.Features.Auth;

public sealed record OperationResult(
    bool Success,
    string? Error);
