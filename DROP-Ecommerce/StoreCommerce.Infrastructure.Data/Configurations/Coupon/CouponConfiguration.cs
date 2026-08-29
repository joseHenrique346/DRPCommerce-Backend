using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Code).HasMaxLength(100);

        builder.Property(e => e.DiscountValue).HasPrecision(18, 2);
        builder.Property(e => e.MinOrderValue).HasPrecision(18, 2);
        builder.Property(e => e.MaxDiscountCap).HasPrecision(18, 2);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListCoupon)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CouponType)
            .WithMany()
            .HasForeignKey(e => e.CouponTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListOrder)
            .WithOne(e => e.Coupon)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.CouponTypeId);
        builder.HasIndex(e => e.Code);
    }
}
