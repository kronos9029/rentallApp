using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalApp.Domain.Entities;
using RentalApp.Domain.Enums;
using BookingEntity = RentalApp.Domain.Entities.Booking;

namespace RentalApp.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<BookingEntity>
{
    public void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(booking => booking.BookingId);
        builder.Property(booking => booking.BookingId).HasColumnName("booking_id").HasMaxLength(36);
        builder.Property(booking => booking.UserId).HasColumnName("user_id").HasMaxLength(36).IsRequired();
        builder.Property(booking => booking.HoldId).HasColumnName("hold_id").HasMaxLength(36);
        builder.Property(booking => booking.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(BookingStatus.Pending);
        builder.Property(booking => booking.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2);
        builder.Property(booking => booking.BookingDate).HasColumnName("booking_date").HasColumnType("date");
        builder.Property(booking => booking.StartAt).HasColumnName("start_at").HasPrecision(6);
        builder.Property(booking => booking.EndAt).HasColumnName("end_at").HasPrecision(6);
        builder.Property(booking => booking.CancellationPolicyId).HasColumnName("cancellation_policy_id").HasMaxLength(36);
        builder.Property(booking => booking.CancelledAt).HasColumnName("cancelled_at").HasPrecision(6);
        builder.Property(booking => booking.ConfirmedAt).HasColumnName("confirmed_at").HasPrecision(6);
        builder.Property(booking => booking.CompletedAt).HasColumnName("completed_at").HasPrecision(6);
        builder.Property(booking => booking.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(booking => booking.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(booking => booking.HoldId).IsUnique().HasDatabaseName("uq_bookings_hold");
        builder.HasIndex(booking => new { booking.UserId, booking.CreatedAt }).HasDatabaseName("ix_bookings_user_created");
        builder.HasIndex(booking => new { booking.Status, booking.BookingDate }).HasDatabaseName("ix_bookings_status_date");
        builder.HasOne(booking => booking.User).WithMany(user => user.Bookings).HasForeignKey(booking => booking.UserId);
        builder.HasOne(booking => booking.Hold).WithOne().HasForeignKey<BookingEntity>(booking => booking.HoldId);
        builder.HasOne(booking => booking.CancellationPolicy).WithMany(policy => policy.Bookings).HasForeignKey(booking => booking.CancellationPolicyId);
    }
}

internal sealed class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("booking_items");
        builder.HasKey(item => item.BookingItemId);
        builder.Property(item => item.BookingItemId).HasColumnName("booking_item_id").HasMaxLength(36);
        builder.Property(item => item.BookingId).HasColumnName("booking_id").HasMaxLength(36).IsRequired();
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
        builder.Property(item => item.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(BookingItemStatus.Active);
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(item => item.BookingId).HasDatabaseName("ix_booking_items_booking");
        builder.HasIndex(item => new { item.CourtId, item.BucketId }).HasDatabaseName("ix_booking_items_court_bucket");
        builder.HasOne(item => item.Booking).WithMany(booking => booking.BookingItems).HasForeignKey(item => item.BookingId);
        builder.HasOne(item => item.Court).WithMany(court => court.BookingItems).HasForeignKey(item => item.CourtId);
        builder.HasOne(item => item.TimeBucket).WithMany(bucket => bucket.BookingItems).HasForeignKey(item => item.BucketId);
    }
}

internal sealed class CheckoutOrderConfiguration : IEntityTypeConfiguration<CheckoutOrder>
{
    public void Configure(EntityTypeBuilder<CheckoutOrder> builder)
    {
        builder.ToTable("checkout_orders");
        builder.HasKey(order => order.OrderId);
        builder.Property(order => order.OrderId).HasColumnName("order_id").HasMaxLength(36);
        builder.Property(order => order.UserId).HasColumnName("user_id").HasMaxLength(36).IsRequired();
        builder.Property(order => order.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CheckoutOrderStatus.Pending);
        builder.Property(order => order.SubtotalAmount).HasColumnName("subtotal_amount").HasPrecision(12, 2);
        builder.Property(order => order.DiscountAmount).HasColumnName("discount_amount").HasPrecision(12, 2);
        builder.Property(order => order.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2);
        builder.Property(order => order.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("VND");
        builder.Property(order => order.ExpiresAt).HasColumnName("expires_at").HasPrecision(6);
        builder.Property(order => order.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.Property(order => order.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
        builder.HasIndex(order => new { order.UserId, order.CreatedAt }).HasDatabaseName("ix_checkout_orders_user_created");
        builder.HasIndex(order => new { order.Status, order.CreatedAt }).HasDatabaseName("ix_checkout_orders_status_created");
        builder.HasOne(order => order.User).WithMany(user => user.CheckoutOrders).HasForeignKey(order => order.UserId);
    }
}

internal sealed class CheckoutOrderItemConfiguration : IEntityTypeConfiguration<CheckoutOrderItem>
{
    public void Configure(EntityTypeBuilder<CheckoutOrderItem> builder)
    {
        builder.ToTable("checkout_order_items");
        builder.HasKey(item => item.OrderItemId);
        builder.Property(item => item.OrderItemId).HasColumnName("order_item_id").HasMaxLength(36);
        builder.Property(item => item.OrderId).HasColumnName("order_id").HasMaxLength(36).IsRequired();
        builder.Property(item => item.BookingId).HasColumnName("booking_id").HasMaxLength(36).IsRequired();
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").HasPrecision(6);
        builder.HasIndex(item => new { item.OrderId, item.BookingId }).IsUnique().HasDatabaseName("uq_checkout_order_booking");
        builder.HasIndex(item => item.BookingId).HasDatabaseName("ix_checkout_order_items_booking");
        builder.HasOne(item => item.Order).WithMany(order => order.OrderItems).HasForeignKey(item => item.OrderId);
        builder.HasOne(item => item.Booking).WithMany(booking => booking.CheckoutOrderItems).HasForeignKey(item => item.BookingId);
    }
}
