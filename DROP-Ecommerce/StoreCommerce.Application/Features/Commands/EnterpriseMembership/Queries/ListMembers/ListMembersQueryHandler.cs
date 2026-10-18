using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListMembersQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListMembersQuery, IReadOnlyList<EnterpriseMembershipResponse>>(context)
{
    protected override async Task<IReadOnlyList<EnterpriseMembershipResponse>> ExecuteAsync(ListMembersQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var members = await data.EnterpriseMemberships.ListAsync(x => x.EnterpriseId == tenant, ct, ignoreQueryFilters: true, tracking: false);

        return members.Select(EnterpriseMembershipResponse.FromEntity).ToList();
    }
}
