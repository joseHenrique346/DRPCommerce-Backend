using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetRolePermissionCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<SetRolePermissionCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(SetRolePermissionCommand command, CancellationToken ct)
    {
        var roleId = command.RoleId;
        var permissionId = command.PermissionId;
        var grant = command.Grant;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;

        if (!await data.AccessRoles.ExistsAsync(x => x.Id == roleId && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true))
            throw NotFound();

        if (grant && !await data.Permissions.ExistsAsync(x => x.Id == permissionId && x.IsActive, ct))
            throw new SecurityFailure(400, "Access.InvalidPermission", "Permissão inválida.");

        var relation = await data.AccessRolePermissions.FindAsync(
            x => x.AccessRoleId == roleId && x.PermissionId == permissionId && x.EnterpriseId == tenant,
            ct,
            ignoreQueryFilters: true);

        if (grant && relation is null)
            data.AccessRolePermissions.Add(AccessRolePermission.Create(tenant, roleId, permissionId));

        if (!grant && relation is not null)
            data.AccessRolePermissions.Remove(relation);

        await InvalidateRoleMembersAsync(roleId, tenant, ct);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
