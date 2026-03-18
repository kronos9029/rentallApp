using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalApp.Domain.Common;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;
using RentalApp.Infrastructure.Security.Authentication;

namespace RentalApp.UnitTests;

public sealed class DbUserAuthServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ReturnsAuthenticatedAdminUser()
    {
        await using var dbContext = CreateDbContext();
        await SeedAdminAsync(dbContext, "Admin123!");
        var service = CreateService(dbContext);

        var result = await service.AuthenticateAsync("admin@local.test", "Admin123!");

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Contains(AppRoles.Admin, result.User!.Roles);
    }

    [Fact]
    public async Task RegisterCustomerAsync_PersistsCustomerRole()
    {
        await using var dbContext = CreateDbContext();
        await SeedRolesAsync(dbContext);
        var service = CreateService(dbContext);

        var result = await service.RegisterCustomerAsync("newuser@local.test", "New User", "Password123!");
        var createdUser = await dbContext.Users
            .Include(user => user.Profile)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Email == "newuser@local.test");

        Assert.True(result.Success);
        Assert.NotNull(createdUser);
        Assert.Equal("New User", createdUser!.Profile!.FullName);
        Assert.Contains(createdUser.UserRoles, userRole => userRole.Role.RoleName == AppRoles.Customer);
    }

    [Fact]
    public async Task ResetPasswordAsync_RejectsExpiredToken()
    {
        await using var dbContext = CreateDbContext();
        await SeedAdminAsync(dbContext, "Admin123!");
        var service = CreateService(dbContext);

        var resetRequest = await service.CreatePasswordResetRequestAsync(
            "admin@local.test",
            TimeSpan.FromMinutes(-1));

        var result = await service.ResetPasswordAsync(
            "admin@local.test",
            resetRequest.Token!,
            "NewAdmin123!");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateProfileAsync_ChangesFullName()
    {
        await using var dbContext = CreateDbContext();
        await SeedAdminAsync(dbContext, "Admin123!");
        var service = CreateService(dbContext);

        var result = await service.UpdateProfileAsync("10000000-0000-0000-0000-000000000001", "Updated Admin");
        var updatedProfile = await dbContext.UserProfiles.SingleAsync(profile => profile.UserId == "10000000-0000-0000-0000-000000000001");

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("Updated Admin", updatedProfile.FullName);
        Assert.Equal("Updated Admin", result.User!.FullName);
    }

    private static DbUserAuthService CreateService(RentalAppDbContext dbContext)
    {
        return new DbUserAuthService(dbContext, new PasswordHasher<AppUser>());
    }

    private static RentalAppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RentalAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RentalAppDbContext(options);
    }

    private static async Task SeedRolesAsync(RentalAppDbContext dbContext)
    {
        dbContext.Roles.AddRange(
            new Role
            {
                RoleId = "role-admin",
                RoleName = AppRoles.Admin,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = "role-customer",
                RoleName = AppRoles.Customer,
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(RentalAppDbContext dbContext, string password)
    {
        await SeedRolesAsync(dbContext);

        var now = DateTime.UtcNow;
        var user = new AppUser
        {
            UserId = "10000000-0000-0000-0000-000000000001",
            Email = "admin@local.test",
            Username = "admin.local",
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);

        dbContext.Users.Add(user);
        dbContext.UserProfiles.Add(new UserProfile
        {
            UserId = user.UserId,
            FullName = "Admin Local",
            CreatedAt = now,
            UpdatedAt = now
        });
        dbContext.UserRoles.Add(new UserRole
        {
            UserId = user.UserId,
            RoleId = "role-admin",
            AssignedAt = now
        });

        await dbContext.SaveChangesAsync();
    }
}
