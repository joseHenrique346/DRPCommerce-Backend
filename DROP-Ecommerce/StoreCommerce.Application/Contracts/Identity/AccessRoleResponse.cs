namespace StoreCommerce.Application.Contracts.Identity;

public sealed record AccessRoleResponse(long Id, long EnterpriseId, string Name, string NormalizedName, string Description, bool IsActive);
