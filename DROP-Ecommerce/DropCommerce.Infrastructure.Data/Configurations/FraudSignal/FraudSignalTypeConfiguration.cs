using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class FraudSignalTypeConfiguration : IEntityTypeConfiguration<FraudSignalType>
{
    public void Configure(EntityTypeBuilder<FraudSignalType> builder)
    {
        builder.ToTable("tipo_sinal_fraude");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "IP duplicado" },
            new { Id = 2L, Description = "Dispositivo duplicado" },
            new { Id = 3L, Description = "Comportamento de bot" },
            new { Id = 4L, Description = "Múltiplas contas" },
            new { Id = 5L, Description = "VPN detectada" },
            new { Id = 6L, Description = "Velocidade anormal" }
        );
    }
}
