using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class QueueSessionStatusConfiguration : IEntityTypeConfiguration<QueueSessionStatus>
{
    public void Configure(EntityTypeBuilder<QueueSessionStatus> builder)
    {
        builder.ToTable("status_sessao_fila");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Ativa" },
            new { Id = 2L, Description = "Expirada" },
            new { Id = 3L, Description = "Invalidada" }
        );
    }
}
