using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using RentalApp.Application.Features.Auth;

namespace RentalApp.Web.Security;

public static class DbAuthenticatedUserClaimsPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
