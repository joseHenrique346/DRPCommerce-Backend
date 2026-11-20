namespace StoreCommerce.Application.Contracts;

public sealed record ProductReactivationResult(
    long EnterpriseId,
    long ContractRevision,
    IReadOnlyList<long> ReactivatedProductIds,
    int PublishedProducts);
