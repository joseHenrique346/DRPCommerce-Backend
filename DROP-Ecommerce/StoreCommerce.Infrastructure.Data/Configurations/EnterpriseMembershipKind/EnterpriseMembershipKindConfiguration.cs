using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EnterpriseMembershipKindConfiguration : IEntityTypeConfiguration<EnterpriseMembershipKind>
{
    public void Configure(EntityTypeBuilder<EnterpriseMembershipKind> builder)
    {
        builder.ToTable("tipo_membro_empresa");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);
        builder.HasData(BaseStaticEntity.GetAll<EnterpriseMembershipKind>()
            .Select(e => new { e.Id, e.Description }));
    }
}
