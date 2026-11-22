using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropReservationStatusConfiguration : IEntityTypeConfiguration<DropReservationStatus>
{
    public void Configure(EntityTypeBuilder<DropReservationStatus> builder)
    {
        builder.ToTable("status_reserva");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Ativa" },
            new { Id = 2L, Description = "Confirmada" },
            new { Id = 3L, Description = "Expirada" },
            new { Id = 4L, Description = "Cancelada" }
        );
    }
}
