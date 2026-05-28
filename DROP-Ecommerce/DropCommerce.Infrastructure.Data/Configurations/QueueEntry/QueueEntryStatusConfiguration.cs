using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class QueueEntryStatusConfiguration : IEntityTypeConfiguration<QueueEntryStatus>
{
    public void Configure(EntityTypeBuilder<QueueEntryStatus> builder)
    {
        builder.ToTable("status_entrada_fila");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Aguardando" },
            new { Id = 2L, Description = "Chamado" },
            new { Id = 3L, Description = "Finalizando compra" },
            new { Id = 4L, Description = "Concluído" },
            new { Id = 5L, Description = "Expirado" },
            new { Id = 6L, Description = "Removido" }
        );
    }
}
