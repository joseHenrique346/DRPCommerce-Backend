using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CouponTypeConfiguration : IEntityTypeConfiguration<CouponType>
{
    public void Configure(EntityTypeBuilder<CouponType> builder)
    {
        builder.ToTable("tipo_cupom");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Percentual" },
            new { Id = 2L, Description = "Valor fixo" }
        );
    }
}
