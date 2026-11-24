namespace StoreCommerce.Application.Contracts;

public sealed record ProductContractImpactRequest(
    long PlanId,
    long? AddOnId,
    long ExpectedContractRevision,
    int CatalogVersion);
