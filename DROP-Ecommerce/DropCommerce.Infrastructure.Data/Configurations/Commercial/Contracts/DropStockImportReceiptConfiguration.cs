using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropStockImportReceiptConfiguration : IEntityTypeConfiguration<DropStockImportReceipt>
{
    public void Configure(EntityTypeBuilder<DropStockImportReceipt> builder)
    {
        builder.ToTable("recebimento_estoque_store");
        builder.HasKey(Receipt => Receipt.Id);
        builder.HasIndex(Receipt => Receipt.OperationId).IsUnique();
        builder.Property(Receipt => Receipt.RejectionCode).HasMaxLength(80);
        builder.HasOne(Receipt => Receipt.CatalogProduct).WithMany().HasForeignKey(Receipt => Receipt.CatalogProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
