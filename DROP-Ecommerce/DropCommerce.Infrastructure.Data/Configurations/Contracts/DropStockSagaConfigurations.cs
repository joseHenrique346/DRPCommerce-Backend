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

public sealed class DropStockReturnIntentConfiguration : IEntityTypeConfiguration<DropStockReturnIntent>
{
    public void Configure(EntityTypeBuilder<DropStockReturnIntent> builder)
    {
        builder.ToTable("devolucao_estoque_store");
        builder.HasKey(Return => Return.Id);
        builder.HasIndex(Return => Return.OperationId).IsUnique();
        builder.HasIndex(Return => Return.CatalogProductId).IsUnique().HasFilter("completed_at IS NULL");
        builder.Property(Return => Return.SnapshotJson).HasColumnType("jsonb");
        builder.HasOne<DropCatalogProduct>().WithMany().HasForeignKey(Return => Return.CatalogProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
