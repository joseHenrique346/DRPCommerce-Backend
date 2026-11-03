using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Code).HasMaxLength(Coupon.CodeMaxLength);

        builder.Property(e => e.DiscountValue).HasPrecision(18, 2);

        // Impede que dois resgates simultâneos usem o mesmo saldo de usos.
        builder.Property(e => e.UsedCount).IsConcurrencyToken();
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

        builder.OwnsMany(e => e.ListCondition, nav =>
        {
            nav.ToTable("condicao_cupom");
            nav.WithOwner().HasForeignKey("CouponId");
            nav.Property<long>("Id").ValueGeneratedOnAdd();
            nav.HasKey("Id");
            nav.Property(e => e.Value).HasPrecision(18, 2);
            nav.Property(e => e.SecondValue).HasPrecision(18, 2);
            nav.HasOne<CouponConditionType>().WithMany().HasForeignKey(e => e.CouponConditionTypeId).OnDelete(DeleteBehavior.Restrict);
            nav.HasOne<CouponConditionOperator>().WithMany().HasForeignKey(e => e.CouponConditionOperatorId).OnDelete(DeleteBehavior.Restrict);
            nav.HasIndex("CouponId");
        });
        builder.Navigation(e => e.ListCondition).HasField("_listCondition");

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.CouponTypeId);

        // Código único por loja entre cupons não excluídos (o código já é gravado normalizado em maiúsculas).
        builder.HasIndex(e => new { e.EnterpriseId, e.Code }).IsUnique().HasFilter("is_deleted = FALSE");
    }
}
