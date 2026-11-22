using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

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
