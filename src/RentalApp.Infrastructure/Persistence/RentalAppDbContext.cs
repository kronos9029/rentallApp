using Microsoft.EntityFrameworkCore;
using RentalApp.Domain.Entities;
using BookingEntity = RentalApp.Domain.Entities.Booking;

namespace RentalApp.Infrastructure.Persistence;

public sealed class RentalAppDbContext(DbContextOptions<RentalAppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<Court> Courts => Set<Court>();

    public DbSet<TimeBucket> TimeBuckets => Set<TimeBucket>();

    public DbSet<CourtBucket> CourtBuckets => Set<CourtBucket>();

    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    public DbSet<CancellationPolicy> CancellationPolicies => Set<CancellationPolicy>();

    public DbSet<Hold> Holds => Set<Hold>();

    public DbSet<HoldItem> HoldItems => Set<HoldItem>();

    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    public DbSet<CheckoutOrder> CheckoutOrders => Set<CheckoutOrder>();

    public DbSet<CheckoutOrderItem> CheckoutOrderItems => Set<CheckoutOrderItem>();

    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentalAppDbContext).Assembly);
    }
}
