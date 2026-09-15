using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class ShipmentStatusConfiguration : IEntityTypeConfiguration<ShipmentStatus>
{
    public void Configure(EntityTypeBuilder<ShipmentStatus> builder)
    {
        builder.ToTable("status_envio");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Em processamento" },
            new { Id = 3L, Description = "Enviado" },
            new { Id = 4L, Description = "Em trânsito" },
            new { Id = 5L, Description = "Entregue" },
            new { Id = 6L, Description = "Cancelado" },
            new { Id = 7L, Description = "Devolvido" }
        );
    }
}
