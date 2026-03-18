using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RentalApp.IntegrationTests;

public sealed class SecuritySmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SecuritySmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:MySql:Host"] = "127.0.0.1",
                    ["Database:MySql:Port"] = "3306",
                    ["Database:MySql:Database"] = "rental_app",
                    ["Database:MySql:User"] = "root",
                    ["Database:MySql:Password"] = string.Empty,
                    ["Database:MySql:TreatTinyAsBoolean"] = "true",
                    ["Database:MySql:AllowPublicKeyRetrieval"] = "true",
                    ["Database:MySql:SslMode"] = "None"
                });
            });
        });
    }

    [Fact]
    public async Task HomePage_ReturnsSecurityHeaders()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task AnonymousUser_IsRedirectedFromProfileToLogin()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/Account/Profile");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Auth/Login", response.Headers.Location!.OriginalString, StringComparison.OrdinalIgnoreCase);
    }
}
