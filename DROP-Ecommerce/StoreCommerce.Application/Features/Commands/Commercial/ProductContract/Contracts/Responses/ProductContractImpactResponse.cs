namespace StoreCommerce.Application.Contracts;

public sealed record ProductContractImpactResponse(
    long EnterpriseId,
    long ContractRevision,
    long TargetPlanId,
    long? TargetAddOnId,
    int TotalProducts,
    int PublicationLimit,
    int CurrentlyPublished,
    int PublishedAfterChange,
    int ProductsToBlock,
    int ExistingBlockedProducts,
    int ExcessTotalProducts,
    bool AffectedProductsTruncated,
    IReadOnlyList<ProductReactivationCandidate> AffectedProducts);
