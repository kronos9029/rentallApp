using Microsoft.Extensions.Options;
using RentalApp.Domain.Common;
using RentalApp.Web.Security;

namespace RentalApp.UnitTests;

public class DevelopmentAuthStoreTests
{
    [Fact]
    public void RegisterCustomer_AddsUserWithCustomerRole()
    {
        var store = CreateStore();

        var result = store.RegisterCustomer("newuser@local.test", "New User", "Password123!");
        var createdUser = store.FindByEmail("newuser@local.test");

        Assert.True(result.Success);
        Assert.NotNull(createdUser);
        Assert.Contains(AppRoles.Customer, createdUser!.Roles);
    }

    [Fact]
    public void ValidateCredentials_ReturnsPrincipalForConfiguredAdminUser()
    {
        var store = CreateStore();

        var isValid = store.ValidateCredentials("admin@local.test", "Admin123!", out var principal);

        Assert.True(isValid);
        Assert.True(principal.IsInRole(AppRoles.Admin));
    }

    [Fact]
    public void ResetPassword_RejectsExpiredToken()
    {
        var store = CreateStore();

        var resetRequest = store.CreatePasswordResetRequest(
            "admin@local.test",
            TimeSpan.FromMinutes(-1));

        var result = store.ResetPassword(
            "admin@local.test",
            resetRequest.Token!,
            "NewAdmin123!");

        Assert.False(result.Success);
    }

    [Fact]
    public void UpdateProfile_ChangesFullNameAndPrincipalClaim()
    {
        var store = CreateStore();

        var result = store.UpdateProfile("admin@local.test", "Updated Admin");
        var updatedUser = store.FindByEmail("admin@local.test");

        Assert.True(result.Success);
        Assert.NotNull(result.Principal);
        Assert.Equal("Updated Admin", updatedUser!.FullName);
        Assert.Equal("Updated Admin", result.Principal!.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value);
    }

    private static DevelopmentAuthStore CreateStore()
    {
        var options = Options.Create(new DevelopmentAuthOptions
        {
            Users =
            [
                new()
                {
                    Email = "admin@local.test",
                    FullName = "Admin Local",
                    Password = "Admin123!",
                    Roles = [AppRoles.Admin]
                }
            ]
        });

        return new DevelopmentAuthStore(options);
    }
}
