using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListEnterprisesQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListEnterprisesQuery, IReadOnlyList<MembershipChoice>>(context)
{
    protected override async Task<IReadOnlyList<MembershipChoice>> ExecuteAsync(ListEnterprisesQuery command, CancellationToken ct)
    {
        return await data.GetMembershipChoicesAsync(actor.UserId, ct);
    }
}
