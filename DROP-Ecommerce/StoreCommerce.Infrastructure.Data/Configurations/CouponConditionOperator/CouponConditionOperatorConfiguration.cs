using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CouponConditionOperatorConfiguration : IEntityTypeConfiguration<CouponConditionOperator>
{
    public void Configure(EntityTypeBuilder<CouponConditionOperator> builder)
    {
        builder.ToTable("operador_condicao_cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(BaseStaticEntity.GetAll<CouponConditionOperator>()
            .Select(item => new { item.Id, item.Description })
            .ToArray<object>());
    }
}
