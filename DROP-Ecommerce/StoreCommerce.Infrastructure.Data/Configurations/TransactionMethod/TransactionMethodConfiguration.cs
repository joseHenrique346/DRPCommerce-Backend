using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class TransactionMethodConfiguration : IEntityTypeConfiguration<TransactionMethod>
{
    public void Configure(EntityTypeBuilder<TransactionMethod> builder)
    {
        builder.ToTable("metodo_transacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Cartão de crédito" },
            new { Id = 2L, Description = "Pix" },
            new { Id = 3L, Description = "Boleto" }
        );
    }
}
