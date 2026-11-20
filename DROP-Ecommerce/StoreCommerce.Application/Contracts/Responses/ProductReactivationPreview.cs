namespace StoreCommerce.Application.Contracts;

public sealed record ProductReactivationPreview(
    long EnterpriseId,
    long ContractRevision,
    string Mode,
    int PublicationLimit,
    int PublishedProducts,
    int AvailablePublicationSlots,
    int EligibleBlockedProducts,
    IReadOnlyList<ProductReactivationCandidate> SelectedProducts);
