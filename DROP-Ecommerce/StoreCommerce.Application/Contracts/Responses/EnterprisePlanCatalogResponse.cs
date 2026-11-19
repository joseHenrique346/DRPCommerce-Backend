namespace StoreCommerce.Application.Contracts;

public sealed record EnterprisePlanCatalogResponse(
    int CatalogVersion,
    string Currency,
    int MaximumActiveAddOns,
    IReadOnlyList<EnterprisePlanResponse> Plans,
    IReadOnlyList<EnterpriseAddOnResponse> AddOns);
