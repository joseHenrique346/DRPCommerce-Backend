using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListDomainsQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<ListDomainsQuery, IReadOnlyList<EnterpriseDomainResponse>>(context)
{
    protected override async Task<IReadOnlyList<EnterpriseDomainResponse>> ExecuteAsync(ListDomainsQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var domains = await data.EnterpriseDomains.ListAsync(x => x.EnterpriseId == tenant, ct, ignoreQueryFilters: true, tracking: false);

        return domains.Select(EnterpriseDomainResponse.FromEntity).ToList();
    }
}
