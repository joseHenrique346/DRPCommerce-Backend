using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Entity;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;

namespace SharedCommerce.Infrastructure.Data.Configurations;

public sealed class AppearanceRevisionConfiguration : IEntityTypeConfiguration<AppearanceRevision>
{
    public void Configure(EntityTypeBuilder<AppearanceRevision> builder)
    {
        builder.ToTable("revisao_aparencia");

        builder.HasKey(AppearanceRevision => AppearanceRevision.Id);
        builder.Property(AppearanceRevision => AppearanceRevision.Id).ValueGeneratedOnAdd();
        builder.Property(AppearanceRevision => AppearanceRevision.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(AppearanceRevision => AppearanceRevision.SettingsJson).HasColumnType("jsonb");
        builder.Property(AppearanceRevision => AppearanceRevision.Checksum).HasMaxLength(80);

        builder.HasAlternateKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.Id })
            .HasName("ak_revision_tenant_id");

        builder.HasAlternateKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.AppearanceId, AppearanceRevision.Id })
            .HasName("ak_revision_tenant_owner_id");

        builder.HasIndex(AppearanceRevision => new { AppearanceRevision.AppearanceId, AppearanceRevision.Number }).IsUnique();

        builder.HasOne(AppearanceRevision => AppearanceRevision.AppearanceRevisionOrigin)
            .WithMany()
            .HasForeignKey(AppearanceRevision => AppearanceRevision.AppearanceRevisionOriginId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_revision_origin");

        builder.HasOne<AppearanceEntity>()
            .WithMany()
            .HasForeignKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.AppearanceId })
            .HasPrincipalKey(Appearance => new { Appearance.EnterpriseId, Appearance.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_revision_appearance");

        builder.HasOne<ThemeRelease>()
            .WithMany()
            .HasForeignKey(AppearanceRevision => AppearanceRevision.ThemeReleaseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_revision_release");

        builder.HasOne<AppearanceRevision>()
            .WithMany()
            .HasForeignKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.RestoredFromRevisionId })
            .HasPrincipalKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_revision_restored_from");
    }
}
