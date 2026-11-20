namespace StoreCommerce.Application.Contracts;

/// <summary>
/// Contagem local no Store. O consumo DROP ainda não foi integrado e nunca é representado como zero conhecido.
/// </summary>
public sealed record EnterpriseContractUsage(
    long TotalStoreProducts,
    long ActiveStoreProducts,
    long InactiveStoreProducts,
    long ActiveAdditionalMembers,
    long PendingInvitations,
    long? DropProducts,
    bool DropUsageAvailable,
    long BlockedStoreProducts,
    long PublishedStoreProducts);
