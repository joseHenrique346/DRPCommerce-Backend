using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CouponConditionTypeConfiguration : IEntityTypeConfiguration<CouponConditionType>
{
    public void Configure(EntityTypeBuilder<CouponConditionType> builder)
    {
        builder.ToTable("tipo_condicao_cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(BaseStaticEntity.GetAll<CouponConditionType>()
            .Select(type => new { type.Id, type.Description })
            .ToArray<object>());
    }
}
