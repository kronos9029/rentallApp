using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RentalApp.Domain.Common;

namespace RentalApp.Web.Security;

public sealed class DevelopmentAuthStore
{
    private readonly ConcurrentDictionary<string, DevelopmentAuthUserRecord> _users;
    private readonly ConcurrentDictionary<string, DevelopmentPasswordResetToken> _resetTokens = new(StringComparer.Ordinal);
    private readonly PasswordHasher<DevelopmentAuthUserRecord> _passwordHasher = new();

    public DevelopmentAuthStore(IOptions<DevelopmentAuthOptions> options)
    {
        var configuredUsers = options.Value.Users ?? [];
        _users = new ConcurrentDictionary<string, DevelopmentAuthUserRecord>(StringComparer.OrdinalIgnoreCase);

        foreach (var user in configuredUsers.Select(ToRecord))
        {
            _users[user.Email] = user;
        }
    }

    public bool ValidateCredentials(string email, string password, out ClaimsPrincipal principal)
    {
        principal = new ClaimsPrincipal();

        if (!_users.TryGetValue(email.Trim(), out var user))
        {
            return false;
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return false;
        }

        principal = CreatePrincipal(user);
        return true;
    }

    public (bool Success, string? Error) RegisterCustomer(string email, string fullName, string password)
    {
        var normalizedEmail = email.Trim();
        if (_users.ContainsKey(normalizedEmail))
        {
            return (false, "Email da ton tai.");
        }

        var user = new DevelopmentAuthUserRecord
        {
            Email = normalizedEmail,
            FullName = string.IsNullOrWhiteSpace(fullName) ? normalizedEmail : fullName.Trim(),
            Roles = [AppRoles.Customer]
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        var added = _users.TryAdd(normalizedEmail, user);
        return added
            ? (true, null)
            : (false, "Khong the tao tai khoan luc nay.");
    }

    public DevelopmentAuthUserRecord? FindByEmail(string email)
    {
        _users.TryGetValue(email.Trim(), out var user);
        return user;
    }

    public PasswordResetRequestResult CreatePasswordResetRequest(string email, TimeSpan? lifetime = null)
    {
        var normalizedEmail = email.Trim();
        if (!_users.TryGetValue(normalizedEmail, out var user))
        {
            return new PasswordResetRequestResult(false, null, null);
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTimeOffset.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(15));

        _resetTokens[token] = new DevelopmentPasswordResetToken
        {
            Email = user.Email,
            Token = token,
            ExpiresAt = expiresAt
        };

        return new PasswordResetRequestResult(
            true,
            token,
            expiresAt);
    }

    public bool CanResetPassword(string email, string token)
    {
        var normalizedEmail = email.Trim();
        return _resetTokens.TryGetValue(token, out var storedToken) &&
            storedToken.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            storedToken.ExpiresAt > DateTimeOffset.UtcNow;
    }

    public (bool Success, string? Error) ResetPassword(string email, string token, string newPassword)
    {
        var normalizedEmail = email.Trim();
        if (!_users.TryGetValue(normalizedEmail, out var user))
        {
            return (false, "Yeu cau reset khong hop le hoac da het han.");
        }

        if (!_resetTokens.TryGetValue(token, out var storedToken) ||
            !storedToken.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
            storedToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return (false, "Yeu cau reset khong hop le hoac da het han.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        _resetTokens.TryRemove(token, out _);
        return (true, null);
    }

    public (bool Success, string? Error, ClaimsPrincipal? Principal) UpdateProfile(string email, string fullName)
    {
        var normalizedEmail = email.Trim();
        if (!_users.TryGetValue(normalizedEmail, out var user))
        {
            return (false, "Khong tim thay tai khoan hien tai.", null);
        }

        user.FullName = string.IsNullOrWhiteSpace(fullName) ? normalizedEmail : fullName.Trim();
        return (true, null, CreatePrincipal(user));
    }

    private DevelopmentAuthUserRecord ToRecord(DevelopmentAuthSeedUser seedUser)
    {
        var user = new DevelopmentAuthUserRecord
        {
            Email = seedUser.Email.Trim(),
            FullName = string.IsNullOrWhiteSpace(seedUser.FullName) ? seedUser.Email.Trim() : seedUser.FullName.Trim(),
            Roles = seedUser.Roles.Count == 0 ? [AppRoles.Customer] : seedUser.Roles
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, seedUser.Password);
        return user;
    }

    private static ClaimsPrincipal CreatePrincipal(DevelopmentAuthUserRecord user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}

public sealed class DevelopmentPasswordResetToken
{
    public string Email { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed record PasswordResetRequestResult(
    bool UserExists,
    string? Token,
    DateTimeOffset? ExpiresAt);
