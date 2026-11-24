using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class InvoiceStatusConfiguration : IEntityTypeConfiguration<InvoiceStatus>
{
    public void Configure(EntityTypeBuilder<InvoiceStatus> builder)
    {
        builder.ToTable("status_fatura");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Autorizada" },
            new { Id = 3L, Description = "Emitida" },
            new { Id = 4L, Description = "Cancelada" }
        );
    }
}
