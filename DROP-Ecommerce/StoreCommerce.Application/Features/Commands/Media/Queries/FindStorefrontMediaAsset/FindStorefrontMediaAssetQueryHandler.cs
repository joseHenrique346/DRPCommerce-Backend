using MediatR;
using SharedCommerce.Application.Interfaces.Appearance;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Interfaces.Media;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class FindStorefrontMediaAssetQueryHandler(
    IMediaAssetStore assets,
    ITenantProvider tenant,
    IAppearanceEntitlements entitlements)
    : IRequestHandler<FindStorefrontMediaAssetQuery, MediaAsset?>
{
    public async Task<MediaAsset?> Handle(FindStorefrontMediaAssetQuery request, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var rights = await entitlements.GetAsync(enterpriseId, ct);

        if (!rights.AllowsLogo)
            return null;

        return await assets.FindAsync(enterpriseId, request.PublicId, ct);
    }
}
