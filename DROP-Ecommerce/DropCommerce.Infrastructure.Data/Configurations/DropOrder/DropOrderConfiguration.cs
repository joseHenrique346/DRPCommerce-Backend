using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropOrderConfiguration : IEntityTypeConfiguration<DropOrder>
{
    public void Configure(EntityTypeBuilder<DropOrder> builder)
    {
        builder.ToTable("pedido");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SubTotal).HasPrecision(18, 2);
        builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.ShippingAddressLine).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ShippingCity).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ShippingState).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ShippingZipCode).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropOrder).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropReservation).WithMany(e => e.ListDropOrder).HasForeignKey(e => e.DropReservationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropCoupon).WithMany(e => e.ListDropOrder).HasForeignKey(e => e.DropCouponId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropOrderStatus).WithMany().HasForeignKey(e => e.DropOrderStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropOrderPaymentStatus).WithMany().HasForeignKey(e => e.DropOrderPaymentStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropOrderItem).WithOne(e => e.DropOrder).HasForeignKey(e => e.DropOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropTransaction).WithOne(e => e.DropOrder).HasForeignKey(e => e.DropOrderId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.DropOrderStatusId);
    }
}
