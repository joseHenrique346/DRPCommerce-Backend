using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("pedido");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SubTotal).HasPrecision(18, 2);
        builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);

        builder.Property(e => e.ShippingAddressLine).HasMaxLength(200);
        builder.Property(e => e.ShippingCity).HasMaxLength(100);
        builder.Property(e => e.ShippingZipCode).HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListOrder)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(e => e.ListOrder)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Coupon)
            .WithMany(e => e.ListOrder)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.OrderStatus)
            .WithMany()
            .HasForeignKey(e => e.OrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.OrderPaymentStatus)
            .WithMany()
            .HasForeignKey(e => e.OrderPaymentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ShippingState)
            .WithMany()
            .HasForeignKey(e => e.ShippingStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListInvoice)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListOrderItem)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListShipment)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListTransaction)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.CouponId);
        builder.HasIndex(e => e.OrderStatusId);
        builder.HasIndex(e => e.OrderPaymentStatusId);
        builder.HasIndex(e => e.ShippingStateId);
    }
}
