using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class DocumentStatusConfiguration : IEntityTypeConfiguration<DocumentStatus>
{
    public void Configure(EntityTypeBuilder<DocumentStatus> builder)
    {
        builder.ToTable("status_documento");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Aguardando validação" },
            new { Id = 3L, Description = "Validado" },
            new { Id = 4L, Description = "Rejeitado" },
            new { Id = 5L, Description = "Expirado" }
        );
    }
}
