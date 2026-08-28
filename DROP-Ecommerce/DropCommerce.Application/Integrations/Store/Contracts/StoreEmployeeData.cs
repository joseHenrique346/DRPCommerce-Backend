namespace DropCommerce.Application.Integrations.Store.Contracts;

public sealed record StoreEmployeeData(long EmployeeId, long EnterpriseId, string FullName, bool IsActive);
