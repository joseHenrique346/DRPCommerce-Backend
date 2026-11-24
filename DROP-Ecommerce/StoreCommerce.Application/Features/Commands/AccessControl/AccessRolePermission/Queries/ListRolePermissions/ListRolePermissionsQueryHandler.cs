using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListRolePermissionsQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListRolePermissionsQuery, IReadOnlyList<AccessRolePermissionResponse>>(context)
{
    protected override async Task<IReadOnlyList<AccessRolePermissionResponse>> ExecuteAsync(ListRolePermissionsQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        return await data.AccessRolePermissions.ListAsync(
            Relation => true,
            Relation => new AccessRolePermissionResponse(Relation.EnterpriseId, Relation.AccessRoleId, Relation.PermissionId),
            ct);
    }
}
