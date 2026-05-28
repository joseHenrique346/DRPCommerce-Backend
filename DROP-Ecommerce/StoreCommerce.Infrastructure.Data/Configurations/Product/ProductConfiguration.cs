using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity.Product;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("produto");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200);
        builder.Property(e => e.Slug).HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.SKU).HasMaxLength(100);
        builder.Property(e => e.BarCode).HasMaxLength(100);
        builder.Property(e => e.Brand).HasMaxLength(200);
        builder.Property(e => e.ImageUrls).HasMaxLength(500);

        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.CostPrice).HasPrecision(18, 2);
        builder.Property(e => e.Weight).HasPrecision(18, 2);
        builder.Property(e => e.Height).HasPrecision(18, 2);
        builder.Property(e => e.Width).HasPrecision(18, 2);
        builder.Property(e => e.Length).HasPrecision(18, 2);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<StoreCommerce.Domain.Entity.Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StoreCommerce.Domain.Entity.Category.Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StoreCommerce.Domain.Entity.Supplier>()
            .WithMany()
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
