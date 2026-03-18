namespace RentalApp.Infrastructure.Persistence.Configuration;

public sealed class MySqlDatabaseOptions
{
    public const string SectionName = "Database:MySql";

    public string Host { get; set; } = "127.0.0.1";

    public uint Port { get; set; } = 3306;

    public string Database { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool TreatTinyAsBoolean { get; set; } = true;

    public bool AllowPublicKeyRetrieval { get; set; } = true;

    public string SslMode { get; set; } = "None";
}
