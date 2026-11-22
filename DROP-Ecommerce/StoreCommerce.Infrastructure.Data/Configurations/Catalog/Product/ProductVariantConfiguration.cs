using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("variante_produto", Table =>
        {
            Table.HasCheckConstraint("ck_variante_reservado", "reserved_quantity >= 0");
            Table.HasCheckConstraint("ck_variante_versao", "version > 0");
        });
        builder.HasKey(Variant => Variant.Id);
        builder.Property(Variant => Variant.Id).ValueGeneratedOnAdd();
        builder.HasAlternateKey(Variant => new { Variant.EnterpriseId, Variant.Id });
        builder.HasAlternateKey(Variant => new { Variant.ProductId, Variant.Id });
        builder.Property(Variant => Variant.Name).HasMaxLength(ProductVariant.NameMaxLength).IsRequired();
        builder.Property(Variant => Variant.SKU).HasMaxLength(Product.SkuMaxLength).IsRequired();
        builder.Property(Variant => Variant.BarCode).HasMaxLength(Product.BarCodeMaxLength);
        builder.Property(Variant => Variant.AttributesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(Variant => Variant.CombinationKey).HasMaxLength(64).IsRequired();
        builder.Property(Variant => Variant.Price).HasPrecision(18, 2);
        builder.Property(Variant => Variant.CostPrice).HasPrecision(18, 2);
        builder.Property(Variant => Variant.Weight).HasPrecision(18, 2);
        builder.Property(Variant => Variant.Height).HasPrecision(18, 2);
        builder.Property(Variant => Variant.Width).HasPrecision(18, 2);
        builder.Property(Variant => Variant.Length).HasPrecision(18, 2);
        builder.Property(Variant => Variant.Version).IsConcurrencyToken();
        builder.Ignore(Variant => Variant.AvailableQuantity);
        builder.HasOne(Variant => Variant.Product).WithMany(Product => Product.ListProductVariant)
            .HasForeignKey(Variant => new { Variant.EnterpriseId, Variant.ProductId })
            .HasPrincipalKey(Product => new { Product.EnterpriseId, Product.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(Variant => new { Variant.EnterpriseId, Variant.SKU }).IsUnique().HasFilter("is_deleted = FALSE");
        builder.HasIndex(Variant => new { Variant.ProductId, Variant.CombinationKey }).IsUnique().HasFilter("is_deleted = FALSE");
        builder.HasIndex(Variant => Variant.ProductId).IsUnique().HasFilter("is_default = TRUE AND is_deleted = FALSE");
    }
}
