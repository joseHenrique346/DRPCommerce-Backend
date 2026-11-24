using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("ativo_midia");
        builder.HasKey(Asset => Asset.Id);
        builder.Property(Asset => Asset.Id).ValueGeneratedOnAdd();
        builder.Property(Asset => Asset.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(Asset => Asset.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(Asset => Asset.Content).HasColumnType("bytea").IsRequired();
        builder.HasIndex(Asset => Asset.PublicId).IsUnique();
        builder.HasIndex(Asset => new { Asset.EnterpriseId, Asset.ContentHash }).IsUnique().HasFilter("is_deleted = FALSE");
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Asset => Asset.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
