namespace RentalApp.Web.Security;

public sealed class DevelopmentAuthUserRecord
{
    public string Email { get; init; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = [];
}
