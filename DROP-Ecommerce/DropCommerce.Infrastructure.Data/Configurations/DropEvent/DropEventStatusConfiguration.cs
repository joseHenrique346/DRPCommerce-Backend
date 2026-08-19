using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropEventStatusConfiguration : IEntityTypeConfiguration<DropEventStatus>
{
    public void Configure(EntityTypeBuilder<DropEventStatus> builder)
    {
        builder.ToTable("status_evento");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Rascunho" },
            new { Id = 2L, Description = "Inscrições abertas" },
            new { Id = 3L, Description = "Inscrições encerradas" },
            new { Id = 4L, Description = "Fila aberta" },
            new { Id = 5L, Description = "Ativo" },
            new { Id = 6L, Description = "Esgotado" },
            new { Id = 7L, Description = "Encerrado" },
            new { Id = 8L, Description = "Cancelado" }
        );
    }
}
