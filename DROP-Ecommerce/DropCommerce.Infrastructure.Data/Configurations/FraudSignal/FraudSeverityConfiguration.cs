using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class FraudSeverityConfiguration : IEntityTypeConfiguration<FraudSeverity>
{
    public void Configure(EntityTypeBuilder<FraudSeverity> builder)
    {
        builder.ToTable("severidade_fraude");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Baixa" },
            new { Id = 2L, Description = "Média" },
            new { Id = 3L, Description = "Alta" },
            new { Id = 4L, Description = "Crítica" }
        );
    }
}
