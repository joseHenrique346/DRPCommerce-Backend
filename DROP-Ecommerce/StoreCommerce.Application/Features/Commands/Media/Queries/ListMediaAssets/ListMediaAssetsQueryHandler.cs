using StoreCommerce.Application.Media;
using MediatR;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Media;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListMediaAssetsQueryHandler(
    ICurrentIdentity actor,
    IMediaAssetStore assets)
    : IRequestHandler<ListMediaAssetsQuery, IReadOnlyList<MediaAssetSummary>>
{
    public async Task<IReadOnlyList<MediaAssetSummary>> Handle(ListMediaAssetsQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        return await assets.ListAsync(actor.EnterpriseId, ct);
    }
}
