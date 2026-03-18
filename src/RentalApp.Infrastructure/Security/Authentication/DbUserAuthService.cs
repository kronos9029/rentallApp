using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalApp.Application.Features.Auth;
using RentalApp.Domain.Common;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;

namespace RentalApp.Infrastructure.Security.Authentication;

public sealed class DbUserAuthService(
    RentalAppDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher) : IUserAuthService
{
    private const int DefaultResetLifetimeMinutes = 15;
    private readonly RentalAppDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<AppUser> _passwordHasher = passwordHasher;

    public async Task<AuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await LoadUserByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || user.Status is not UserStatus.Active)
        {
            return new AuthenticationResult(false, "Thong tin dang nhap khong hop le.", null);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return new AuthenticationResult(false, "Thong tin dang nhap khong hop le.", null);
        }

        var now = DateTime.UtcNow;
        user.LastLoginAt = now;
        user.UpdatedAt = now;

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new AuthenticationResult(true, null, CreateSnapshot(user));
    }

    public async Task<OperationResult> RegisterCustomerAsync(
        string email,
        string fullName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
        if (existingUser)
        {
            return new OperationResult(false, "Email da ton tai.");
        }

        var customerRole = await _dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(role => role.RoleName == AppRoles.Customer, cancellationToken);
        if (customerRole is null)
        {
            return new OperationResult(false, "Customer role chua duoc khoi tao.");
        }

        var now = DateTime.UtcNow;
        var user = new AppUser
        {
            UserId = Guid.NewGuid().ToString(),
            Email = normalizedEmail,
            Username = await GenerateUniqueUsernameAsync(normalizedEmail, cancellationToken),
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        var profile = new UserProfile
        {
            UserId = user.UserId,
            FullName = BuildDisplayName(normalizedEmail, fullName),
            CreatedAt = now,
            UpdatedAt = now
        };

        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = customerRole.RoleId,
            AssignedAt = now
        };

        _dbContext.Users.Add(user);
        _dbContext.UserProfiles.Add(profile);
        _dbContext.UserRoles.Add(userRole);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult(true, null);
    }

    public async Task<AuthenticatedUser?> GetProfileAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserId = userId.Trim();
        var user = await LoadUserByIdAsync(normalizedUserId, cancellationToken);
        return user is null ? null : CreateSnapshot(user);
    }

    public async Task<AuthenticatedUserResult> UpdateProfileAsync(
        string userId,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserId = userId.Trim();
        var user = await LoadUserByIdAsync(normalizedUserId, cancellationToken);
        if (user is null)
        {
            return new AuthenticatedUserResult(false, "Khong tim thay tai khoan hien tai.", null);
        }

        var now = DateTime.UtcNow;
        var displayName = BuildDisplayName(user.Email, fullName);
        if (user.Profile is null)
        {
            user.Profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = displayName,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
        else
        {
            user.Profile.FullName = displayName;
            user.Profile.UpdatedAt = now;
        }

        user.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthenticatedUserResult(true, null, CreateSnapshot(user));
    }

    public async Task<PasswordResetRequestResult> CreatePasswordResetRequestAsync(
        string email,
        TimeSpan? lifetime = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);
        if (user is null || user.Status is not UserStatus.Active)
        {
            return new PasswordResetRequestResult(false, null, null);
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.Add(lifetime ?? TimeSpan.FromMinutes(DefaultResetLifetimeMinutes));
        var outstandingTokens = await _dbContext.PasswordResetTokens
            .Where(token => token.UserId == user.UserId && token.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var outstandingToken in outstandingTokens)
        {
            outstandingToken.UsedAt = now;
        }

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var resetToken = new PasswordResetToken
        {
            TokenId = Guid.NewGuid().ToString(),
            UserId = user.UserId,
            TokenHash = HashResetToken(rawToken),
            ExpiresAt = expiresAt,
            CreatedAt = now
        };

        _dbContext.PasswordResetTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PasswordResetRequestResult(
            true,
            rawToken,
            new DateTimeOffset(expiresAt, TimeSpan.Zero));
    }

    public async Task<bool> CanResetPasswordAsync(
        string email,
        string token,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var tokenHash = HashResetToken(token);
        var now = DateTime.UtcNow;

        return await _dbContext.PasswordResetTokens
            .Include(resetToken => resetToken.User)
            .AnyAsync(
                resetToken => resetToken.TokenHash == tokenHash &&
                    resetToken.UsedAt == null &&
                    resetToken.ExpiresAt > now &&
                    resetToken.User.Email == normalizedEmail &&
                    resetToken.User.Status == UserStatus.Active,
                cancellationToken);
    }

    public async Task<OperationResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var tokenHash = HashResetToken(token);
        var now = DateTime.UtcNow;

        var resetToken = await _dbContext.PasswordResetTokens
            .Include(candidate => candidate.User)
            .SingleOrDefaultAsync(
                candidate => candidate.TokenHash == tokenHash &&
                    candidate.UsedAt == null &&
                    candidate.User.Email == normalizedEmail,
                cancellationToken);

        if (resetToken is null || resetToken.ExpiresAt <= now || resetToken.User.Status is not UserStatus.Active)
        {
            return new OperationResult(false, "Yeu cau reset khong hop le hoac da het han.");
        }

        resetToken.User.PasswordHash = _passwordHasher.HashPassword(resetToken.User, newPassword);
        resetToken.User.UpdatedAt = now;
        resetToken.UsedAt = now;

        var outstandingTokens = await _dbContext.PasswordResetTokens
            .Where(candidate => candidate.UserId == resetToken.UserId && candidate.TokenId != resetToken.TokenId && candidate.UsedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var outstandingToken in outstandingTokens)
        {
            outstandingToken.UsedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new OperationResult(true, null);
    }

    private async Task<AppUser?> LoadUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(user => user.Profile)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    private async Task<AppUser?> LoadUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(user => user.Profile)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    private async Task<string> GenerateUniqueUsernameAsync(string email, CancellationToken cancellationToken)
    {
        var usernameBase = BuildUsernameBase(email);
        var candidate = usernameBase;
        var suffix = 0;

        while (await _dbContext.Users.AnyAsync(user => user.Username == candidate, cancellationToken))
        {
            suffix++;
            var suffixText = $".{suffix}";
            var prefixLength = Math.Min(usernameBase.Length, 100 - suffixText.Length);
            candidate = usernameBase[..prefixLength] + suffixText;
        }

        return candidate;
    }

    private static AuthenticatedUser CreateSnapshot(AppUser user)
    {
        var roles = user.UserRoles
            .Select(userRole => userRole.Role.RoleName)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        return new AuthenticatedUser(
            user.UserId,
            user.Email,
            BuildDisplayName(user.Email, user.Profile?.FullName),
            roles);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string BuildDisplayName(string email, string? fullName)
    {
        return string.IsNullOrWhiteSpace(fullName)
            ? email
            : fullName.Trim();
    }

    private static string BuildUsernameBase(string email)
    {
        var localPart = email.Split('@', 2)[0].Trim().ToLowerInvariant();
        var builder = new StringBuilder(localPart.Length);

        foreach (var character in localPart)
        {
            if (char.IsLetterOrDigit(character) || character is '.' or '_' or '-')
            {
                builder.Append(character);
            }
            else if (builder.Length == 0 || builder[^1] != '.')
            {
                builder.Append('.');
            }
        }

        var candidate = builder.ToString().Trim('.', '-', '_');
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "user";
        }

        return candidate.Length <= 100
            ? candidate
            : candidate[..100];
    }

    private static string HashResetToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token.Trim());
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
