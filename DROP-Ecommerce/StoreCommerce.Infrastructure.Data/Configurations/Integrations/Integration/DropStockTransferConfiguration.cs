using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class DropStockTransferConfiguration : IEntityTypeConfiguration<DropStockTransfer>
{
    public void Configure(EntityTypeBuilder<DropStockTransfer> builder)
    {
        builder.ToTable("transferencia_estoque_drop");
        builder.HasKey(Transfer => Transfer.Id);
        builder.Property(Transfer => Transfer.IdempotencyKey).HasMaxLength(100);
        builder.Property(Transfer => Transfer.SnapshotJson).HasColumnType("jsonb");
        builder.HasIndex(Transfer => Transfer.OperationId).IsUnique();
        builder.HasIndex(Transfer => new { Transfer.EnterpriseId, Transfer.IdempotencyKey }).IsUnique();
        builder.HasIndex(Transfer => new { Transfer.EnterpriseId, Transfer.ProductId }).IsUnique().HasFilter("returned_at IS NULL");
        builder.HasOne<Product>().WithMany().HasForeignKey(Transfer => Transfer.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Transfer => Transfer.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
