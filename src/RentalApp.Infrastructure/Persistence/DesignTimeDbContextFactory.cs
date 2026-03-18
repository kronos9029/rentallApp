using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using RentalApp.Infrastructure.Persistence.Configuration;
using RentalApp.Infrastructure.Security.Secrets;

namespace RentalApp.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RentalAppDbContext>
{
    public RentalAppDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var rootPath = ResolveProjectRoot();
        var configuration = new ConfigurationManager();
        configuration.SetBasePath(rootPath);
        configuration.AddJsonFile("src/RentalApp.Web/appsettings.json", optional: false, reloadOnChange: false);
        configuration.AddJsonFile($"src/RentalApp.Web/appsettings.{environmentName}.json", optional: true, reloadOnChange: false);
        configuration.DecryptMarkedValuesFromEnvironment();

        var databaseOptions = configuration.GetSection(MySqlDatabaseOptions.SectionName).Get<MySqlDatabaseOptions>()
            ?? throw new InvalidOperationException("Database:MySql configuration was not configured.");
        var connectionString = MySqlConnectionStringFactory.Build(databaseOptions);
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<RentalAppDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            serverVersion);

        return new RentalAppDbContext(optionsBuilder.Options);
    }

    private static string ResolveProjectRoot()
    {
        var current = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(current, "RentalApp.slnx")))
        {
            return current;
        }

        var directory = new DirectoryInfo(current);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RentalApp.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not resolve project root containing RentalApp.slnx.");
    }
}
