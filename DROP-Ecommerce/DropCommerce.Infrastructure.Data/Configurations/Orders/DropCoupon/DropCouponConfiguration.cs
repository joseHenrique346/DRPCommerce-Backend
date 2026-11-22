using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropCouponConfiguration : IEntityTypeConfiguration<DropCoupon>
{
    public void Configure(EntityTypeBuilder<DropCoupon> builder)
    {
        builder.ToTable("cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Code).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DiscountValue).HasPrecision(18, 2);
        builder.Property(e => e.MinOrderValue).HasPrecision(18, 2);
        builder.Property(e => e.MaxDiscountCap).HasPrecision(18, 2);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropCoupon).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropCouponType).WithMany().HasForeignKey(e => e.DropCouponTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropOrder).WithOne(e => e.DropCoupon).HasForeignKey(e => e.DropCouponId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.Code);
    }
}
