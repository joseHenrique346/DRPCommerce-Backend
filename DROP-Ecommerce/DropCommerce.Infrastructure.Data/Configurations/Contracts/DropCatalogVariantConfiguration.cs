using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropCatalogVariantConfiguration : IEntityTypeConfiguration<DropCatalogVariant>
{
    public void Configure(EntityTypeBuilder<DropCatalogVariant> builder)
    {
        builder.ToTable("catalogo_variante");
        builder.HasKey(Variant => Variant.Id);
        builder.Property(Variant => Variant.SKU).HasMaxLength(100);
        builder.Property(Variant => Variant.OptionsJson).HasColumnType("jsonb");
        builder.Property(Variant => Variant.Price).HasPrecision(12, 2);
        builder.HasIndex(Variant => new { Variant.CatalogProductId, Variant.SKU }).IsUnique();
        builder.HasOne(Variant => Variant.CatalogProduct).WithMany(Product => Product.Variants)
            .HasForeignKey(Variant => Variant.CatalogProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
