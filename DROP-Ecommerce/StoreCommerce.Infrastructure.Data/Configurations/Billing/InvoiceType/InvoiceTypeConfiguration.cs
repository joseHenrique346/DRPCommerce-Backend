using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class InvoiceTypeConfiguration : IEntityTypeConfiguration<InvoiceType>
{
    public void Configure(EntityTypeBuilder<InvoiceType> builder)
    {
        builder.ToTable("tipo_fatura");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Nota Fiscal Eletrônica" },
            new { Id = 2L, Description = "Nota Fiscal de Consumidor Eletrônica" },
            new { Id = 3L, Description = "Nota Fiscal de Serviço Eletrônica" },
            new { Id = 4L, Description = "Nota Fiscal" }
        );
    }
}
