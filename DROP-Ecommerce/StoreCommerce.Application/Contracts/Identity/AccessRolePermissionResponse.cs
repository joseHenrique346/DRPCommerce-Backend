namespace StoreCommerce.Application.Contracts.Identity;

public sealed record AccessRolePermissionResponse(long EnterpriseId, long AccessRoleId, long PermissionId);
