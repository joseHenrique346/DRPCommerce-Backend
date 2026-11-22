using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Entity;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;

namespace SharedCommerce.Infrastructure.Data.Configurations;

public sealed class AppearanceEditSessionConfiguration : IEntityTypeConfiguration<AppearanceEditSession>
{
    public void Configure(EntityTypeBuilder<AppearanceEditSession> builder)
    {
        builder.ToTable("sessao_edicao_aparencia");

        builder.HasKey(AppearanceEditSession => AppearanceEditSession.Id);
        builder.Property(AppearanceEditSession => AppearanceEditSession.Id).ValueGeneratedOnAdd();
        builder.Property(AppearanceEditSession => AppearanceEditSession.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(AppearanceEditSession => AppearanceEditSession.SettingsJson).HasColumnType("jsonb");
        builder.Property(AppearanceEditSession => AppearanceEditSession.PreviewJson).HasColumnType("jsonb");
        builder.Property(AppearanceEditSession => AppearanceEditSession.Version).IsConcurrencyToken();

        builder.HasIndex(AppearanceEditSession => AppearanceEditSession.AppearanceId).IsUnique().HasFilter("is_open = TRUE");
        builder.HasIndex(AppearanceEditSession => new { AppearanceEditSession.IsOpen, AppearanceEditSession.ExpiresAt });

        builder.HasOne(AppearanceEditSession => AppearanceEditSession.AppearanceEditSessionStatus)
            .WithMany()
            .HasForeignKey(AppearanceEditSession => AppearanceEditSession.AppearanceEditSessionStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_edit_session_status");

        builder.HasOne<AppearanceEntity>()
            .WithMany()
            .HasForeignKey(AppearanceEditSession => new { AppearanceEditSession.EnterpriseId, AppearanceEditSession.AppearanceId })
            .HasPrincipalKey(Appearance => new { Appearance.EnterpriseId, Appearance.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_edit_appearance");

        builder.HasOne<ThemeRelease>()
            .WithMany()
            .HasForeignKey(AppearanceEditSession => AppearanceEditSession.ThemeReleaseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_edit_release");

        builder.HasOne<AppearanceRevision>()
            .WithMany()
            .HasForeignKey(AppearanceEditSession => new { AppearanceEditSession.EnterpriseId, AppearanceEditSession.AppearanceId, AppearanceEditSession.BaseRevisionId })
            .HasPrincipalKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.AppearanceId, AppearanceRevision.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_edit_base_revision");

        builder.HasOne<AppearanceRevision>()
            .WithMany()
            .HasForeignKey(AppearanceEditSession => new { AppearanceEditSession.EnterpriseId, AppearanceEditSession.AppearanceId, AppearanceEditSession.RestoredFromRevisionId })
            .HasPrincipalKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.AppearanceId, AppearanceRevision.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_edit_restore_revision");
    }
}
