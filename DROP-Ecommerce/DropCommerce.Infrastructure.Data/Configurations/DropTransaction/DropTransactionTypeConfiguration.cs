using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropTransactionTypeConfiguration : IEntityTypeConfiguration<DropTransactionType>
{
    public void Configure(EntityTypeBuilder<DropTransactionType> builder)
    {
        builder.ToTable("tipo_transacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pagamento" },
            new { Id = 2L, Description = "Reembolso" },
            new { Id = 3L, Description = "Reembolso parcial" },
            new { Id = 4L, Description = "Chargeback" }
        );
    }
}
