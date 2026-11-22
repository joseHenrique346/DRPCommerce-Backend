namespace StoreCommerce.Application.Contracts.Identity;

public sealed record DepartmentResponse(long Id, long EnterpriseId, string Name, string Description, bool IsActive);
