using MediatR;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Media;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class FindMediaAssetQueryHandler(
    ICurrentIdentity actor,
    IMediaAssetStore assets)
    : IRequestHandler<FindMediaAssetQuery, MediaAsset?>
{
    public async Task<MediaAsset?> Handle(FindMediaAssetQuery command, CancellationToken ct)
    {
        var id = command.Id;
        actor.RequirePrimaryOwner();

        return await assets.FindAsync(actor.EnterpriseId, id, ct);
    }
}
