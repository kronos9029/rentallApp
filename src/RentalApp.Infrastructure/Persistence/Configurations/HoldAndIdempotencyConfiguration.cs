using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;

namespace RentalApp.Infrastructure.Persistence.Configurations;

internal sealed class HoldConfiguration : IEntityTypeConfiguration<Hold>
{
    public void Configure(EntityTypeBuilder<Hold> builder)
    {
        builder.ToTable("holds");
        builder.HasKey(hold => hold.HoldId);
        builder.Property(hold => hold.HoldId).HasColumnName("hold_id").HasMaxLength(36);
        builder.Property(hold => hold.UserId).HasColumnName("user_id").HasMaxLength(36).IsRequired();
        builder.Property(hold => hold.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(HoldStatus.Active);
        builder.Property(hold => hold.ExpiresAt).HasColumnName("expires_at").HasPrecision(6);
        builder.Property(hold => hold.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(hold => hold.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(hold => new { hold.Status, hold.ExpiresAt }).HasDatabaseName("ix_holds_status_expires");
        builder.HasIndex(hold => new { hold.UserId, hold.CreatedAt }).HasDatabaseName("ix_holds_user_created");
        builder.HasOne(hold => hold.User)
            .WithMany(user => user.Holds)
            .HasForeignKey(hold => hold.UserId);
    }
}

internal sealed class HoldItemConfiguration : IEntityTypeConfiguration<HoldItem>
{
    public void Configure(EntityTypeBuilder<HoldItem> builder)
    {
        builder.ToTable("hold_items");
        builder.HasKey(item => item.HoldItemId);
        builder.Property(item => item.HoldItemId).HasColumnName("hold_item_id").HasMaxLength(36);
        builder.Property(item => item.HoldId).HasColumnName("hold_id").HasMaxLength(36).IsRequired();
        builder.Property(item => item.CourtId).HasColumnName("court_id").HasMaxLength(36).IsRequired();
        builder.Property(item => item.BucketId).HasColumnName("bucket_id").HasMaxLength(36).IsRequired();
        builder.Property(item => item.BookingMode)
            .HasColumnName("booking_mode")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(item => item.SlotQty).HasColumnName("slot_qty").HasDefaultValue((byte)1);
        builder.Property(item => item.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2);
        builder.Property(item => item.LineTotal).HasColumnName("line_total").HasPrecision(12, 2);
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.HasIndex(item => new { item.HoldId, item.CourtId, item.BucketId, item.BookingMode })
            .IsUnique()
            .HasDatabaseName("uq_hold_items_scope");
        builder.HasIndex(item => new { item.CourtId, item.BucketId }).HasDatabaseName("ix_hold_items_court_bucket");
        builder.HasOne(item => item.Hold)
            .WithMany(hold => hold.HoldItems)
            .HasForeignKey(item => item.HoldId);
        builder.HasOne(item => item.Court)
            .WithMany(court => court.HoldItems)
            .HasForeignKey(item => item.CourtId);
        builder.HasOne(item => item.TimeBucket)
            .WithMany(bucket => bucket.HoldItems)
            .HasForeignKey(item => item.BucketId);
    }
}

internal sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(record => record.IdempotencyId);
        builder.Property(record => record.IdempotencyId).HasColumnName("idempotency_id").HasMaxLength(36);
        builder.Property(record => record.Scope).HasColumnName("scope").HasMaxLength(36).IsRequired();
        builder.Property(record => record.Endpoint).HasColumnName("endpoint").HasMaxLength(80).IsRequired();
        builder.Property(record => record.IdempotencyKeyValue).HasColumnName("idempotency_key").HasMaxLength(64).IsRequired();
        builder.Property(record => record.RequestHash).HasColumnName("request_hash").HasMaxLength(64).IsRequired();
        builder.Property(record => record.ResponseStatus).HasColumnName("response_status");
        builder.Property(record => record.ResponseSnapshot).HasColumnName("response_snapshot").HasColumnType("longtext");
        builder.Property(record => record.ExpiresAt).HasColumnName("expires_at").HasPrecision(6);
        builder.Property(record => record.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.HasIndex(record => new { record.Scope, record.Endpoint, record.IdempotencyKeyValue })
            .IsUnique()
            .HasDatabaseName("uq_idempotency_scope_endpoint_key");
        builder.HasIndex(record => record.ExpiresAt).HasDatabaseName("ix_idempotency_expires");
    }
}
