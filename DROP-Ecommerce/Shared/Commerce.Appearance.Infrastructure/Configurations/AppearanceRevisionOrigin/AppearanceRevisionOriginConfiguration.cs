using Commerce.Appearance.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Appearance.Infrastructure.Configurations;

public sealed class AppearanceRevisionOriginConfiguration : IEntityTypeConfiguration<AppearanceRevisionOrigin>
{
    public void Configure(EntityTypeBuilder<AppearanceRevisionOrigin> builder)
    {
        builder.ToTable("origem_revisao_aparencia");

        builder.HasKey(Origin => Origin.Id);
        builder.Property(Origin => Origin.Id).ValueGeneratedNever();
        builder.Property(Origin => Origin.Description).IsRequired().HasMaxLength(100);
        builder.Property(Origin => Origin.Code).IsRequired().HasMaxLength(20);

        builder.HasIndex(Origin => Origin.Code).IsUnique();

        builder.HasData(BaseStaticEntity.GetAll<AppearanceRevisionOrigin>()
            .Select(Origin => new { Origin.Id, Origin.Code, Origin.Description }));
    }
}
