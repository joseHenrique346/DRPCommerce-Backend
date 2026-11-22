using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Contracts;

public sealed record EnterpriseEntitlements(
    long PlanId,
    long? AddOnId,
    bool AllowsStore,
    bool AllowsDrop,
    bool AllowsCustomDomain,
    bool AllowsLogoCustomization,
    bool AllowsFullCustomization,
    int StoreProductLimit,
    int DropProductLimit,
    int EmployeeLimit,
    int StoreThemeLimit,
    int DropThemeLimit)
{
    public static EnterpriseEntitlements Resolve(long planId, long? addOnId)
    {
        var plan = EnterprisePlan.FromId(planId);
        var addOn = addOnId.HasValue ? EnterpriseAddOn.FromId(addOnId.Value) : null;

        if (addOn is not null && !addOn.IsEligibleFor(plan.Id))
            throw new ArgumentException("O adicional não está disponível para este plano.", nameof(addOnId));

        return new EnterpriseEntitlements(
            plan.Id,
            addOn?.Id,
            plan.AllowsVitrine,
            plan.IncludesDropModule,
            plan.AllowsCustomDomain,
            plan.AllowsLogoCustomization,
            plan.AllowsFullCustomization,
            checked(plan.StoreProductLimit + Extra("store_products")),
            checked(plan.DropProductLimit + Extra("drop_products")),
            checked(plan.EmployeeLimit + Extra("employees")),
            plan.StoreThemeLimit,
            plan.DropThemeLimit);

        int Extra(string resourceCode)
        {
            return addOn?.ResourceCode == resourceCode ? addOn.Quantity : 0;
        }
    }
}
