using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Contracts;

public static class EnterpriseContractResponseMapper
{
    public static EnterprisePlanResponse Plan(EnterprisePlan plan)
    {
        return new EnterprisePlanResponse(plan.Id, plan.Code, plan.Description, plan.MonthlyPrice,
            EnterpriseEntitlements.Resolve(plan.Id, null));
    }

    public static EnterpriseAddOnResponse AddOn(EnterpriseAddOn addOn)
    {
        return new EnterpriseAddOnResponse(addOn.Id, addOn.Code, addOn.Description,
            addOn.ResourceCode, addOn.Quantity, addOn.MonthlyPrice, addOn.EligiblePlanIds);
    }

    public static CurrentEnterpriseContractResponse Current(EnterpriseContractState state)
    {
        return new CurrentEnterpriseContractResponse(
            state.EnterpriseId,
            state.Revision,
            state.CatalogVersion,
            EnterpriseCommercialCatalog.Currency,
            Plan(EnterprisePlan.FromId(state.PlanId)),
            state.AddOnId.HasValue ? AddOn(EnterpriseAddOn.FromId(state.AddOnId.Value)) : null,
            EnterpriseEntitlements.Resolve(state.PlanId, state.AddOnId));
    }

    public static EnterpriseContractChangeResponse Change(EnterpriseContractChange change)
    {
        return new EnterpriseContractChangeResponse(
            change.Id,
            change.EnterpriseId,
            change.Status == EnterpriseContractChangeStatus.Requested ? "requested" : "superseded",
            change.PlanId,
            change.AddOnId,
            change.ExpectedContractRevision,
            change.CatalogVersion,
            change.Currency,
            change.PlanMonthlyPrice,
            change.AddOnMonthlyPrice,
            change.PlanMonthlyPrice + change.AddOnMonthlyPrice,
            change.RequestedAt,
            change.SupersededAt,
            GrantsAccess: false);
    }
}
