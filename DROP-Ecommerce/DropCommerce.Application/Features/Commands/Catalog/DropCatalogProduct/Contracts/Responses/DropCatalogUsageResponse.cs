namespace DropCommerce.Application.Contracts;

public sealed record DropCatalogUsageResponse(
    long ContractRevision,
    bool AllowsDrop,
    int ProductLimit,
    int CatalogTotal,
    bool CanCreate,
    int PublishedPrincipalEvents,
    int BlockedParticipations,
    int AwaitingStockTransfer,
    int TransferringStock);
