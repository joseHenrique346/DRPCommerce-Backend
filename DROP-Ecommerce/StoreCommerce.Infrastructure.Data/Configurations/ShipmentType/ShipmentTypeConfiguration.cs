using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class ShipmentTypeConfiguration : IEntityTypeConfiguration<ShipmentType>
{
    public void Configure(EntityTypeBuilder<ShipmentType> builder)
    {
        builder.ToTable("tipo_envio");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Normal" },
            new { Id = 2L, Description = "Expresso" },
            new { Id = 3L, Description = "Econômico" },
            new { Id = 4L, Description = "Retirada no local" }
        );
    }
}
