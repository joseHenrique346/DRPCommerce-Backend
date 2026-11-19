using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class EnterpriseAddOnConfiguration : IEntityTypeConfiguration<EnterpriseAddOn>
{
    public void Configure(EntityTypeBuilder<EnterpriseAddOn> builder)
    {
        builder.ToTable("adicional_empresa");
        builder.HasKey(AddOn => AddOn.Id);
        builder.Property(AddOn => AddOn.Id).ValueGeneratedNever();
        builder.Property(AddOn => AddOn.Description).IsRequired().HasMaxLength(100);
        builder.Property(AddOn => AddOn.Code).IsRequired().HasMaxLength(40);
        builder.Property(AddOn => AddOn.ResourceCode).IsRequired().HasMaxLength(30);
        builder.Property(AddOn => AddOn.Quantity).IsRequired();
        builder.Property(AddOn => AddOn.MonthlyPrice).HasPrecision(12, 2);
        builder.Ignore(AddOn => AddOn.EligiblePlanIds);
        builder.HasIndex(AddOn => AddOn.Code).IsUnique();
        builder.HasData(BaseStaticEntity.GetAll<EnterpriseAddOn>().Select(AddOn => new
        {
            AddOn.Id,
            AddOn.Description,
            AddOn.Code,
            AddOn.ResourceCode,
            AddOn.Quantity,
            AddOn.MonthlyPrice
        }));
    }
}
