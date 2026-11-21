using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropContractStateConfiguration : IEntityTypeConfiguration<DropContractState>
{
    public void Configure(EntityTypeBuilder<DropContractState> builder)
    {
        builder.ToTable("contrato_comercial");
        builder.HasKey(State => State.Id);
        builder.HasIndex(State => State.EnterpriseId).IsUnique();
    }
}

public sealed class DropCommercialActivityConfiguration : IEntityTypeConfiguration<DropCommercialActivity>
{
    public void Configure(EntityTypeBuilder<DropCommercialActivity> builder)
    {
        builder.ToTable("atividade_comercial");
        builder.HasKey(Activity => Activity.Id);
        builder.Property(Activity => Activity.Action).HasMaxLength(80);
        builder.HasIndex(Activity => new { Activity.EnterpriseId, Activity.Id });
    }
}

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
