namespace RentalApp.Web.Security;

public sealed class DevelopmentAuthOptions
{
    public const string SectionName = "DevelopmentAuth";

    public bool Enabled { get; set; } = true;

    public List<DevelopmentAuthSeedUser> Users { get; set; } = [];
}

public sealed class DevelopmentAuthSeedUser
{
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}
