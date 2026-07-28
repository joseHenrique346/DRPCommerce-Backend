namespace DropCommerce.Application.Integrations.Store.Contracts;

public sealed record StoreProductData(long ProductId, long EnterpriseId, string Name, string SKU, bool IsActive);
