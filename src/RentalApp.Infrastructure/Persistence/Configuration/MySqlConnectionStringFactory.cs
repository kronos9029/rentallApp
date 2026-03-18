using System.Text;

namespace RentalApp.Infrastructure.Persistence.Configuration;

internal static class MySqlConnectionStringFactory
{
    public static string Build(MySqlDatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new InvalidOperationException("Database:MySql:Host was not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Database))
        {
            throw new InvalidOperationException("Database:MySql:Database was not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.User))
        {
            throw new InvalidOperationException("Database:MySql:User was not configured.");
        }

        var builder = new StringBuilder();
        builder.Append("Server=").Append(options.Host.Trim()).Append(';');
        builder.Append("Port=").Append(options.Port).Append(';');
        builder.Append("Database=").Append(options.Database.Trim()).Append(';');
        builder.Append("User=").Append(options.User.Trim()).Append(';');
        builder.Append("Password=").Append(options.Password).Append(';');
        builder.Append("TreatTinyAsBoolean=").Append(options.TreatTinyAsBoolean ? "true" : "false").Append(';');
        builder.Append("AllowPublicKeyRetrieval=").Append(options.AllowPublicKeyRetrieval ? "true" : "false").Append(';');
        builder.Append("SslMode=").Append(options.SslMode.Trim()).Append(';');
        return builder.ToString();
    }
}
