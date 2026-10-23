using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Infrastructure.Data.Configurations;

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
