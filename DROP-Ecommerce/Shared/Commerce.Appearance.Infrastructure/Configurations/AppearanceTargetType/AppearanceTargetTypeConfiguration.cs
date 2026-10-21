using Commerce.Appearance.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Appearance.Infrastructure.Configurations;

public sealed class AppearanceTargetTypeConfiguration : IEntityTypeConfiguration<AppearanceTargetType>
{
    public void Configure(EntityTypeBuilder<AppearanceTargetType> builder)
    {
        builder.ToTable("tipo_alvo_aparencia");

        builder.HasKey(TargetType => TargetType.Id);
        builder.Property(TargetType => TargetType.Id).ValueGeneratedNever();
        builder.Property(TargetType => TargetType.Description).IsRequired().HasMaxLength(100);
        builder.Property(TargetType => TargetType.Code).IsRequired().HasMaxLength(20);

        builder.HasIndex(TargetType => TargetType.Code).IsUnique();

        builder.HasData(BaseStaticEntity.GetAll<AppearanceTargetType>()
            .Select(TargetType => new { TargetType.Id, TargetType.Code, TargetType.Description }));
    }
}
