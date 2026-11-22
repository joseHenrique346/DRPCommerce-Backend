using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class EnterprisePlanAddOnConfiguration : IEntityTypeConfiguration<EnterprisePlanAddOn>
{
    public void Configure(EntityTypeBuilder<EnterprisePlanAddOn> builder)
    {
        builder.ToTable("plano_adicional_empresa");
        builder.HasKey(Offer => new { Offer.PlanId, Offer.AddOnId });
        builder.HasOne<EnterprisePlan>().WithMany().HasForeignKey(Offer => Offer.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseAddOn>().WithMany().HasForeignKey(Offer => Offer.AddOnId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(BaseStaticEntity.GetAll<EnterpriseAddOn>()
            .SelectMany(AddOn => AddOn.EligiblePlanIds.Select(PlanId => new { PlanId, AddOnId = AddOn.Id })));
    }
}
