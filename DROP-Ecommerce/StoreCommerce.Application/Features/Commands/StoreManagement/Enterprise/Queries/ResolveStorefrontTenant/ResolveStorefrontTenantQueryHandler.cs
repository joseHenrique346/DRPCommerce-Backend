using MediatR;
using SharedCommerce.Domain.Contracts.Storefront;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ResolveStorefrontTenantQueryHandler(IEnterpriseRepository enterprises)
    : IRequestHandler<ResolveStorefrontTenantQuery, long?>
{
    public async Task<long?> Handle(ResolveStorefrontTenantQuery request, CancellationToken ct)
    {
        var slug = StorefrontSlug.Normalize(request.Slug);

        if (slug is null)
            return null;

        return await enterprises.FindActiveStorefrontIdAsync(slug, ct);
    }
}
