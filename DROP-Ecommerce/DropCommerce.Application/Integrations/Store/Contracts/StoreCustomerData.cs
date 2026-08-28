namespace DropCommerce.Application.Integrations.Store.Contracts;

public sealed record StoreCustomerData(long CustomerId, long EnterpriseId, string FullName, bool IsActive);
