using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropNotificationTypeConfiguration : IEntityTypeConfiguration<DropNotificationType>
{
    public void Configure(EntityTypeBuilder<DropNotificationType> builder)
    {
        builder.ToTable("tipo_notificacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Inscrição confirmada" },
            new { Id = 2L, Description = "Abertura da fila" },
            new { Id = 3L, Description = "Chamado na fila" },
            new { Id = 4L, Description = "Reserva expirando" },
            new { Id = 5L, Description = "Pedido confirmado" },
            new { Id = 6L, Description = "Disponível na lista de espera" }
        );
    }
}
