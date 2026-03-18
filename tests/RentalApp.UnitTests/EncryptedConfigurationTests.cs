using Microsoft.Extensions.Configuration;
using RentalApp.Infrastructure.Security.Secrets;

namespace RentalApp.UnitTests;

public sealed class EncryptedConfigurationTests
{
    [Fact]
    public void DecryptMarkedValuesFromEnvironment_LoadsEncryptedScalarValue()
    {
        var key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var encryptedConnectionString = EncryptedConfigurationExtensions.EncryptValue(
            "Server=encrypted-host;Port=3306;Database=rental_app;",
            key);

        var originalKey = Environment.GetEnvironmentVariable(EncryptedConfigurationExtensions.EncryptionKeyEnvironmentVariable);

        try
        {
            Environment.SetEnvironmentVariable(EncryptedConfigurationExtensions.EncryptionKeyEnvironmentVariable, key);
            var configurationManager = new ConfigurationManager();
            configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RentalApp"] = encryptedConnectionString
            });
            configurationManager.DecryptMarkedValuesFromEnvironment();

            Assert.Equal(
                "Server=encrypted-host;Port=3306;Database=rental_app;",
                configurationManager.GetConnectionString("RentalApp"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(EncryptedConfigurationExtensions.EncryptionKeyEnvironmentVariable, originalKey);
        }
    }

    [Fact]
    public void DecryptMarkedValuesFromEnvironment_LeavesPlainValuesUntouched()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Email:Smtp:Host"] = "smtp.gmail.com"
        });

        configurationManager.DecryptMarkedValuesFromEnvironment();

        Assert.Equal("smtp.gmail.com", configurationManager["Email:Smtp:Host"]);
    }
}
