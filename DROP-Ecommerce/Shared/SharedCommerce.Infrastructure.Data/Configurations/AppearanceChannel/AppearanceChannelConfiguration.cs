using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Infrastructure.Data.Configurations;

public sealed class AppearanceChannelConfiguration : IEntityTypeConfiguration<AppearanceChannel>
{
    public void Configure(EntityTypeBuilder<AppearanceChannel> builder)
    {
        builder.ToTable("canal_aparencia");

        builder.HasKey(Channel => Channel.Id);
        builder.Property(Channel => Channel.Id).ValueGeneratedNever();
        builder.Property(Channel => Channel.Description).IsRequired().HasMaxLength(100);
        builder.Property(Channel => Channel.Code).IsRequired().HasMaxLength(10);

        builder.HasIndex(Channel => Channel.Code).IsUnique();

        builder.HasData(BaseStaticEntity.GetAll<AppearanceChannel>()
            .Select(Channel => new { Channel.Id, Channel.Code, Channel.Description }));
    }
}
