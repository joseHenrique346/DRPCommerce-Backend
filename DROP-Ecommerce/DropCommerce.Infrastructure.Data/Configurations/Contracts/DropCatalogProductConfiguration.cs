using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropCatalogProductConfiguration : IEntityTypeConfiguration<DropCatalogProduct>
{
    public void Configure(EntityTypeBuilder<DropCatalogProduct> builder)
    {
        builder.ToTable("catalogo_produto");
        builder.HasKey(Product => Product.Id);
        builder.Property(Product => Product.Name).HasMaxLength(255);
        builder.Property(Product => Product.Description).HasMaxLength(10000);
        builder.HasIndex(Product => new { Product.EnterpriseId, Product.StoreProductId }).IsUnique();
    }
}
