using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListSessionsQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListSessionsQuery, IReadOnlyList<SessionDescription>>(context)
{
    protected override async Task<IReadOnlyList<SessionDescription>> ExecuteAsync(ListSessionsQuery command, CancellationToken ct)
    {
        var customer = command.Customer;

        var accountId = customer ? actor.CustomerId : actor.UserId;
        var family = actor.FamilyId;
        long? tenant = customer ? actor.EnterpriseId : null;

        return await data.Sessions.ListAsync(accountId, family, customer, tenant, Now, ct);
    }
}
