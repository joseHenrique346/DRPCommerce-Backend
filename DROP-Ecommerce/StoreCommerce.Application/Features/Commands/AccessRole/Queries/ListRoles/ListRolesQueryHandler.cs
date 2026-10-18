using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListRolesQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListRolesQuery, IReadOnlyList<AccessRoleResponse>>(context)
{
    protected override async Task<IReadOnlyList<AccessRoleResponse>> ExecuteAsync(ListRolesQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var roles = await data.AccessRoles.ListAsync(x => x.EnterpriseId == tenant, ct, ignoreQueryFilters: true, tracking: false);

        return roles.Select(AccessRoleResponse.FromEntity).ToList();
    }
}
