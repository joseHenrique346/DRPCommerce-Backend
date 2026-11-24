namespace StoreCommerce.Domain.Contracts;

public sealed record EnterpriseContractState(
    long EnterpriseId,
    long PlanId,
    long? AddOnId,
    long Revision,
    int CatalogVersion);
