using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class TransactionStatusConfiguration : IEntityTypeConfiguration<TransactionStatus>
{
    public void Configure(EntityTypeBuilder<TransactionStatus> builder)
    {
        builder.ToTable("status_transacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Autorizado" },
            new { Id = 3L, Description = "Capturado" },
            new { Id = 4L, Description = "Falhou" },
            new { Id = 5L, Description = "Cancelado" },
            new { Id = 6L, Description = "Reembolsado" }
        );
    }
}
