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

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
