using Commerce.Appearance.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Appearance.Infrastructure.Configurations;

public sealed class AppearanceEditSessionStatusConfiguration : IEntityTypeConfiguration<AppearanceEditSessionStatus>
{
    public void Configure(EntityTypeBuilder<AppearanceEditSessionStatus> builder)
    {
        builder.ToTable("status_sessao_edicao_aparencia");

        builder.HasKey(Status => Status.Id);
        builder.Property(Status => Status.Id).ValueGeneratedNever();
        builder.Property(Status => Status.Description).IsRequired().HasMaxLength(100);
        builder.Property(Status => Status.Code).IsRequired().HasMaxLength(20);

        builder.HasIndex(Status => Status.Code).IsUnique();

        builder.HasData(BaseStaticEntity.GetAll<AppearanceEditSessionStatus>()
            .Select(Status => new { Status.Id, Status.Code, Status.Description }));
    }
}
