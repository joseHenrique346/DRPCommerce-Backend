using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListInvitationsQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListInvitationsQuery, IReadOnlyList<InvitationSummary>>(context)
{
    protected override async Task<IReadOnlyList<InvitationSummary>> ExecuteAsync(ListInvitationsQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;

        return await data.EnterpriseInvitations.ListAsync(
            x => x.EnterpriseId == tenant,
            x => new InvitationSummary(x.Id, x.InvitedNormalizedEmail, x.InvitedKindId, x.ExpiresAt, x.AcceptedAt, x.RevokedAt),
            ct,
            ignoreQueryFilters: true);
    }
}
