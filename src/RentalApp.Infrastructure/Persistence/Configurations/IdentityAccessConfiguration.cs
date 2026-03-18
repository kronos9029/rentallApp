using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalApp.Domain.Entities;
using RentalApp.Infrastructure.Persistence.Seed;

namespace RentalApp.Infrastructure.Persistence.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.UserId);
        builder.Property(user => user.UserId).HasColumnName("user_id").HasMaxLength(36);
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(191).IsRequired();
        builder.Property(user => user.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        builder.Property(user => user.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
        builder.Property(user => user.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(user => user.LastLoginAt).HasColumnName("last_login_at").HasPrecision(6);
        builder.Property(user => user.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(user => user.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.Username).IsUnique();
        builder.HasData(AppSeedData.SeedAdminUser, AppSeedData.SeedCustomerUser);
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(role => role.RoleId);
        builder.Property(role => role.RoleId).HasColumnName("role_id").HasMaxLength(36);
        builder.Property(role => role.RoleName).HasColumnName("role_name").HasMaxLength(50).IsRequired();
        builder.Property(role => role.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.HasIndex(role => role.RoleName).IsUnique();
        builder.HasData(AppSeedData.Roles);
    }
}

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
        builder.Property(userRole => userRole.UserId).HasColumnName("user_id").HasMaxLength(36);
        builder.Property(userRole => userRole.RoleId).HasColumnName("role_id").HasMaxLength(36);
        builder.Property(userRole => userRole.AssignedAt).HasColumnName("assigned_at").HasPrecision(6);
        builder.HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId);
        builder.HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId);
        builder.HasData(AppSeedData.SeedAdminUserRole, AppSeedData.SeedCustomerUserRole);
    }
}

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(profile => profile.UserId);
        builder.Property(profile => profile.UserId).HasColumnName("user_id").HasMaxLength(36);
        builder.Property(profile => profile.FullName).HasColumnName("full_name").HasMaxLength(150);
        builder.Property(profile => profile.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        builder.Property(profile => profile.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(profile => profile.EmergencyContact).HasColumnName("emergency_contact").HasMaxLength(50);
        builder.Property(profile => profile.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(profile => profile.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasOne(profile => profile.User)
            .WithOne(user => user.Profile)
            .HasForeignKey<UserProfile>(profile => profile.UserId);
        builder.HasData(AppSeedData.SeedAdminProfile, AppSeedData.SeedCustomerProfile);
    }
}

internal sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.HasKey(token => token.TokenId);
        builder.Property(token => token.TokenId).HasColumnName("token_id").HasMaxLength(36);
        builder.Property(token => token.UserId).HasColumnName("user_id").HasMaxLength(36).IsRequired();
        builder.Property(token => token.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(token => token.ExpiresAt).HasColumnName("expires_at").HasPrecision(6);
        builder.Property(token => token.UsedAt).HasColumnName("used_at").HasPrecision(6);
        builder.Property(token => token.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.UserId);
        builder.HasIndex(token => token.ExpiresAt);
        builder.HasOne(token => token.User)
            .WithMany(user => user.PasswordResetTokens)
            .HasForeignKey(token => token.UserId);
    }
}
