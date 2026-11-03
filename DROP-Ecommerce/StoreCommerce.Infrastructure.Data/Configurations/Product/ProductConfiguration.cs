using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("produto");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.HasAlternateKey(Product => new { Product.EnterpriseId, Product.Id });

        builder.Property(e => e.Name).HasMaxLength(Product.NameMaxLength);
        builder.Property(e => e.Slug).HasMaxLength(Product.SlugMaxLength);
        builder.Property(e => e.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(e => e.SKU).HasMaxLength(Product.SkuMaxLength);
        builder.Property(e => e.BarCode).HasMaxLength(Product.BarCodeMaxLength);
        builder.Property(e => e.Brand).HasMaxLength(Product.BrandMaxLength);
        builder.Property(e => e.ImageUrls).HasMaxLength(Product.ImageUrlsMaxLength);

        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.CostPrice).HasPrecision(18, 2);
        builder.Property(e => e.Weight).HasPrecision(18, 2);
        builder.Property(e => e.Height).HasPrecision(18, 2);
        builder.Property(e => e.Width).HasPrecision(18, 2);
        builder.Property(e => e.Length).HasPrecision(18, 2);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListProduct)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(e => e.ListProduct)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany(e => e.ListProduct)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListOrderItem)
            .WithOne(e => e.Product)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.SupplierId);
        builder.HasIndex(Product => new { Product.EnterpriseId, Product.Slug }).IsUnique().HasFilter("is_deleted = FALSE");
        builder.HasIndex(Product => new { Product.EnterpriseId, Product.SKU }).IsUnique().HasFilter("is_deleted = FALSE");
    }
}
