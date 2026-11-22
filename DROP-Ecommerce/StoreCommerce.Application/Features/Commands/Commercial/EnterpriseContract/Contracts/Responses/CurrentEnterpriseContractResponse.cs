using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Contracts;

public sealed record CurrentEnterpriseContractResponse(
    long EnterpriseId,
    long Revision,
    int CatalogVersion,
    string Currency,
    EnterprisePlanResponse Plan,
    EnterpriseAddOnResponse? AddOn,
    EnterpriseEntitlements Entitlements);
