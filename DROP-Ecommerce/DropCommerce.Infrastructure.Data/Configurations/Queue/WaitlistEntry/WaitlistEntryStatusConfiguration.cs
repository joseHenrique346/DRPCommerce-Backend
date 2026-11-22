using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class WaitlistEntryStatusConfiguration : IEntityTypeConfiguration<WaitlistEntryStatus>
{
    public void Configure(EntityTypeBuilder<WaitlistEntryStatus> builder)
    {
        builder.ToTable("status_lista_espera");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Aguardando" },
            new { Id = 2L, Description = "Notificado" },
            new { Id = 3L, Description = "Expirado" },
            new { Id = 4L, Description = "Atendido" }
        );
    }
}
