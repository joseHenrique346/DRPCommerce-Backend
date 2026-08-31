using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropCouponTypeConfiguration : IEntityTypeConfiguration<DropCouponType>
{
    public void Configure(EntityTypeBuilder<DropCouponType> builder)
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
