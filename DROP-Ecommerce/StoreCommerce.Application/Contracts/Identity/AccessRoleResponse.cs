namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record AccessRoleResponse(long Id, long EnterpriseId, string Name, string NormalizedName, string Description, bool IsActive);
