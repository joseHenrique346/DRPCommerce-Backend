using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppearanceEntity = Commerce.Appearance.Domain.Appearance;

namespace Commerce.Appearance.Infrastructure.Configurations;

public sealed class AppearanceConfiguration : IEntityTypeConfiguration<AppearanceEntity>
{
    public void Configure(EntityTypeBuilder<AppearanceEntity> builder)
    {
        builder.ToTable("aparencia", Table => Table.HasCheckConstraint("ck_appearance_target_event",
            $"(appearance_target_type_id = {AppearanceTargetType.DropSpecific.Id} AND drop_event_id IS NOT NULL AND drop_event_id > 0) OR " +
            $"(appearance_target_type_id IN ({AppearanceTargetType.Store.Id}, {AppearanceTargetType.DropDefault.Id}) AND drop_event_id IS NULL)"));

        builder.HasKey(Appearance => Appearance.Id);
        builder.Property(Appearance => Appearance.Id).ValueGeneratedOnAdd();
        builder.Property(Appearance => Appearance.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(Appearance => Appearance.MigrationFailureCode).HasMaxLength(120);
        builder.Property(Appearance => Appearance.Version).IsConcurrencyToken();

        builder.HasAlternateKey(Appearance => new { Appearance.EnterpriseId, Appearance.Id })
            .HasName("ak_appearance_tenant_id");

        builder.HasIndex(Appearance => new { Appearance.EnterpriseId, Appearance.AppearanceTargetTypeId })
            .IsUnique()
            .HasFilter("drop_event_id IS NULL")
            .HasDatabaseName("ux_appearance_default_target");

        builder.HasIndex(Appearance => new { Appearance.EnterpriseId, Appearance.AppearanceTargetTypeId, Appearance.DropEventId })
            .IsUnique()
            .HasFilter("drop_event_id IS NOT NULL")
            .HasDatabaseName("ux_appearance_specific_target");

        builder.HasOne(Appearance => Appearance.AppearanceTargetType)
            .WithMany()
            .HasForeignKey(Appearance => Appearance.AppearanceTargetTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_appearance_target_type");

        builder.HasOne<AppearanceRevision>()
            .WithMany()
            .HasForeignKey(Appearance => new { Appearance.EnterpriseId, Appearance.Id, Appearance.PublishedRevisionId })
            .HasPrincipalKey(AppearanceRevision => new { AppearanceRevision.EnterpriseId, AppearanceRevision.AppearanceId, AppearanceRevision.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_appearance_published_revision");

        // Id também participa da FK da publicação; mantém a geração explícita no PostgreSQL.
        builder.Property(Appearance => Appearance.Id).UseIdentityByDefaultColumn();
    }
}
