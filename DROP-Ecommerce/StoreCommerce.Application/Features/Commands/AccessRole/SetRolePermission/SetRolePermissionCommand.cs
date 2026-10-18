using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SetRolePermissionCommand(long RoleId, long PermissionId, bool Grant) : IIdentityRequest<bool>;
