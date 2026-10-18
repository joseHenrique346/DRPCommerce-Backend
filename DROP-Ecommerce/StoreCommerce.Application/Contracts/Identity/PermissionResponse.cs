namespace StoreCommerce.Application.Contracts.Identity;

public sealed record PermissionResponse(long Id, string Code, string Name, string Description, string Module, bool IsActive);
