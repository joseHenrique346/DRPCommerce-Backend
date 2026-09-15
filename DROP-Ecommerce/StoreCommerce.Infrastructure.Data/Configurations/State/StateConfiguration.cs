using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("estado");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Acre" },
            new { Id = 2L, Description = "Alagoas" },
            new { Id = 3L, Description = "Amapá" },
            new { Id = 4L, Description = "Amazonas" },
            new { Id = 5L, Description = "Bahia" },
            new { Id = 6L, Description = "Ceará" },
            new { Id = 7L, Description = "Distrito Federal" },
            new { Id = 8L, Description = "Espírito Santo" },
            new { Id = 9L, Description = "Goiás" },
            new { Id = 10L, Description = "Maranhão" },
            new { Id = 11L, Description = "Mato Grosso" },
            new { Id = 12L, Description = "Mato Grosso do Sul" },
            new { Id = 13L, Description = "Minas Gerais" },
            new { Id = 14L, Description = "Pará" },
            new { Id = 15L, Description = "Paraíba" },
            new { Id = 16L, Description = "Paraná" },
            new { Id = 17L, Description = "Pernambuco" },
            new { Id = 18L, Description = "Piauí" },
            new { Id = 19L, Description = "Rio de Janeiro" },
            new { Id = 20L, Description = "Rio Grande do Norte" },
            new { Id = 21L, Description = "Rio Grande do Sul" },
            new { Id = 22L, Description = "Rondônia" },
            new { Id = 23L, Description = "Roraima" },
            new { Id = 24L, Description = "Santa Catarina" },
            new { Id = 25L, Description = "São Paulo" },
            new { Id = 26L, Description = "Sergipe" },
            new { Id = 27L, Description = "Tocantins" }
        );
    }
}
