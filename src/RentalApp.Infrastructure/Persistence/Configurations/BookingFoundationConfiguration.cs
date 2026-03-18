using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence.Seed;

namespace RentalApp.Infrastructure.Persistence.Configurations;

internal sealed class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("courts");
        builder.HasKey(court => court.CourtId);
        builder.Property(court => court.CourtId).HasColumnName("court_id").HasMaxLength(36);
        builder.Property(court => court.CourtCode).HasColumnName("court_code").HasMaxLength(20).IsRequired();
        builder.Property(court => court.CourtName).HasColumnName("court_name").HasMaxLength(100).IsRequired();
        builder.Property(court => court.SortOrder).HasColumnName("sort_order");
        builder.Property(court => court.IsActive).HasColumnName("is_active");
        builder.Property(court => court.MaintenanceReason).HasColumnName("maintenance_reason").HasMaxLength(255);
        builder.Property(court => court.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(court => court.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(court => court.CourtCode).IsUnique();
        builder.HasIndex(court => court.IsActive);
        builder.HasData(AppSeedData.Courts);
    }
}

internal sealed class TimeBucketConfiguration : IEntityTypeConfiguration<TimeBucket>
{
    public void Configure(EntityTypeBuilder<TimeBucket> builder)
    {
        builder.ToTable("time_buckets");
        builder.HasKey(bucket => bucket.BucketId);
        builder.Property(bucket => bucket.BucketId).HasColumnName("bucket_id").HasMaxLength(36);
        builder.Property(bucket => bucket.StartAt).HasColumnName("start_at").HasPrecision(6);
        builder.Property(bucket => bucket.EndAt).HasColumnName("end_at").HasPrecision(6);
        builder.Property(bucket => bucket.DurationMin).HasColumnName("duration_min");
        builder.Property(bucket => bucket.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.HasIndex(bucket => new { bucket.StartAt, bucket.EndAt }).IsUnique();
        builder.HasIndex(bucket => bucket.StartAt);
    }
}

internal sealed class CourtBucketConfiguration : IEntityTypeConfiguration<CourtBucket>
{
    public void Configure(EntityTypeBuilder<CourtBucket> builder)
    {
        builder.ToTable("court_buckets");
        builder.HasKey(bucket => new { bucket.CourtId, bucket.BucketId });
        builder.Property(bucket => bucket.CourtId).HasColumnName("court_id").HasMaxLength(36);
        builder.Property(bucket => bucket.BucketId).HasColumnName("bucket_id").HasMaxLength(36);
        builder.Property(bucket => bucket.Mode).HasColumnName("mode").HasConversion<string>().HasMaxLength(20).HasDefaultValue(CourtBucketMode.None);
        builder.Property(bucket => bucket.SharedCapacity).HasColumnName("shared_capacity").HasDefaultValue((byte)8);
        builder.Property(bucket => bucket.SharedReserved).HasColumnName("shared_reserved").HasDefaultValue((byte)0);
        builder.Property(bucket => bucket.PrivateHoldId).HasColumnName("private_hold_id").HasMaxLength(36);
        builder.Property(bucket => bucket.LockVersion).HasColumnName("lock_version").HasDefaultValue((uint)0);
        builder.Property(bucket => bucket.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(bucket => bucket.Mode);
        builder.HasOne(bucket => bucket.Court)
            .WithMany(court => court.CourtBuckets)
            .HasForeignKey(bucket => bucket.CourtId);
        builder.HasOne(bucket => bucket.TimeBucket)
            .WithMany(timeBucket => timeBucket.CourtBuckets)
            .HasForeignKey(bucket => bucket.BucketId);
    }
}
