namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropCommercialRights(long EnterpriseId, long ContractRevision, int CatalogVersion,
    bool AllowsDrop, int ProductLimit);
