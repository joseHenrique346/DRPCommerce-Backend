using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListMembershipRolesQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListMembershipRolesQuery, IReadOnlyList<MembershipAccessRoleResponse>>(context)
{
    protected override async Task<IReadOnlyList<MembershipAccessRoleResponse>> ExecuteAsync(ListMembershipRolesQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        return await data.MembershipAccessRoles.ListAsync(
            Relation => true,
            Relation => new MembershipAccessRoleResponse(Relation.EnterpriseId, Relation.EnterpriseMembershipId, Relation.AccessRoleId),
            ct);
    }
}
