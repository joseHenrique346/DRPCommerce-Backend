using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropOrderStatusConfiguration : IEntityTypeConfiguration<DropOrderStatus>
{
    public void Configure(EntityTypeBuilder<DropOrderStatus> builder)
    {
        builder.ToTable("status_pedido");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Confirmado" },
            new { Id = 3L, Description = "Em processamento" },
            new { Id = 4L, Description = "Enviado" },
            new { Id = 5L, Description = "Entregue" },
            new { Id = 6L, Description = "Cancelado" },
            new { Id = 7L, Description = "Reembolsado" }
        );
    }
}
