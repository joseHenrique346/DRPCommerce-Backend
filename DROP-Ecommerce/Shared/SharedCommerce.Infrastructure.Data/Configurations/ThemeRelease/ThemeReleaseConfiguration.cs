using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Entity;

namespace SharedCommerce.Infrastructure.Data.Configurations;

public sealed class ThemeReleaseConfiguration : IEntityTypeConfiguration<ThemeRelease>
{
    public void Configure(EntityTypeBuilder<ThemeRelease> builder)
    {
        builder.ToTable("versao_tema");

        builder.HasKey(ThemeRelease => ThemeRelease.Id);
        builder.Property(ThemeRelease => ThemeRelease.Id).ValueGeneratedOnAdd();
        builder.Property(ThemeRelease => ThemeRelease.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ThemeRelease => ThemeRelease.Key).HasMaxLength(80);
        builder.Property(ThemeRelease => ThemeRelease.Version).HasMaxLength(40);
        builder.Property(ThemeRelease => ThemeRelease.Name).HasMaxLength(200);
        builder.Property(ThemeRelease => ThemeRelease.Checksum).HasMaxLength(80);

        builder.Property(ThemeRelease => ThemeRelease.SchemaJson).HasColumnType("jsonb");
        builder.Property(ThemeRelease => ThemeRelease.DefaultsJson).HasColumnType("jsonb");
        builder.Property(ThemeRelease => ThemeRelease.MigrationJson).HasColumnType("jsonb");
        builder.Ignore(ThemeRelease => ThemeRelease.IsDeprecated);

        builder.HasIndex(ThemeRelease => new { ThemeRelease.AppearanceChannelId, ThemeRelease.Key, ThemeRelease.Version }).IsUnique();

        builder.HasOne(ThemeRelease => ThemeRelease.AppearanceChannel)
            .WithMany()
            .HasForeignKey(ThemeRelease => ThemeRelease.AppearanceChannelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_theme_channel");

        builder.HasOne<ThemeRelease>()
            .WithMany()
            .HasForeignKey(ThemeRelease => ThemeRelease.ReplacementReleaseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_theme_replacement");
    }
}
