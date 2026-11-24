using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropNotificationStatusConfiguration : IEntityTypeConfiguration<DropNotificationStatus>
{
    public void Configure(EntityTypeBuilder<DropNotificationStatus> builder)
    {
        builder.ToTable("status_notificacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Agendado" },
            new { Id = 2L, Description = "Enviado" },
            new { Id = 3L, Description = "Entregue" },
            new { Id = 4L, Description = "Falhou" },
            new { Id = 5L, Description = "Devolvido" }
        );
    }
}
