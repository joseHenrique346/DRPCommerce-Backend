namespace StoreCommerce.Domain.StaticEntity;

public sealed class EnterprisePlanAddOn
{
    public long PlanId { get; private set; }
    public long AddOnId { get; private set; }

    private EnterprisePlanAddOn()
    {
    }

    public EnterprisePlanAddOn(long planId, long addOnId)
    {
        if (!EnterpriseAddOn.FromId(addOnId).IsEligibleFor(planId))
            throw new ArgumentException("Adicional incompatível com o plano.");

        PlanId = planId;
        AddOnId = addOnId;
    }
}
