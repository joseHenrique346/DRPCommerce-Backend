using Commerce.Security;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SaveRoleCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<SaveRoleCommand, AccessRoleResponse>(context)
{
    protected override async Task<AccessRoleResponse> ExecuteAsync(SaveRoleCommand command, CancellationToken ct)
    {
        var id = command.Id;
        var request = command.Request;

        var ids = request.PermissionIds ?? [];
        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;

        if (ids.Count != ids.Distinct().Count() || await data.Permissions.CountAsync(x => ids.Contains(x.Id) && x.IsActive, ct) != ids.Count)
            throw new SecurityFailure(400, "Access.InvalidPermissions", "Permissões inválidas ou inativas.");

        AccessRole role;

        if (id.HasValue)
        {
            role = await data.AccessRoles.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();
            role.UpdateDetails(request.Name, request.Description, request.IsActive);
        }
        else
        {
            role = AccessRole.Create(tenant, request.Name, request.Description, request.IsActive);
            data.AccessRoles.Add(role);
            await unitOfWork.CommitAsync(ct);
        }

        var current = await data.AccessRolePermissions.ListAsync(x => x.AccessRoleId == role.Id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
        data.AccessRolePermissions.RemoveRange(current.Where(x => !ids.Contains(x.PermissionId)));

        foreach (var permissionId in ids.Except(current.Select(x => x.PermissionId)))
            data.AccessRolePermissions.Add(AccessRolePermission.Create(tenant, role.Id, permissionId));

        await InvalidateRoleMembersAsync(role.Id, tenant, ct);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return AccessRoleResponse.FromEntity(role);
    }
}
