using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Entity.Coupon;

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

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
