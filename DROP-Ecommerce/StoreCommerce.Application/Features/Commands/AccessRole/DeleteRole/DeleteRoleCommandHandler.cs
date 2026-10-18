using Commerce.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteRoleCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<DeleteRoleCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeleteRoleCommand command, CancellationToken ct)
    {
        var id = command.Id;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var role = await data.AccessRoles.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();

        if (await data.MembershipAccessRoles.ExistsAsync(x => x.AccessRoleId == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ||
            await data.InvitationAccessRoles.ExistsAsync(x => x.AccessRoleId == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Access.RoleInUse", "Perfil em uso. Desative-o em vez de excluir.");

        data.AccessRolePermissions.RemoveRange(await data.AccessRolePermissions.ListAsync(
            x => x.AccessRoleId == id && x.EnterpriseId == tenant,
            ct,
            ignoreQueryFilters: true));
        data.AccessRoles.Remove(role);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
