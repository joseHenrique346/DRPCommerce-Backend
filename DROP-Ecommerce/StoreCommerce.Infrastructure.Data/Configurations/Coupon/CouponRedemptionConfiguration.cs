using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("resgate_cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.Coupon).WithMany().HasForeignKey(e => e.CouponId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => new { e.CouponId, e.CustomerId });

        // Um pedido tem no máximo um resgate ativo.
        builder.HasIndex(e => e.OrderId).IsUnique().HasFilter("released_at IS NULL");
    }
}
