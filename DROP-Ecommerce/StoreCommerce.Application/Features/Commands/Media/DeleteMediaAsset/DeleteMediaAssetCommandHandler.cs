using StoreCommerce.Application.Media;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Media;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteMediaAssetCommandHandler(
    ICurrentIdentity actor,
    IIdentityUnitOfWork unitOfWork,
    IIdentityOperationScope identities,
    MediaWriteScope writes,
    IMediaAssetStore assets,
    MediaOwnerWriteAccess access)
    : IRequestHandler<DeleteMediaAssetCommand>
{
    public async Task Handle(DeleteMediaAssetCommand command, CancellationToken ct)
    {
        var id = command.Id;
        actor.RequirePrimaryOwner();
        using var operation = identities.Begin();
        await using var transaction = await access.BeginAsync(ct);
        var asset = await assets.FindAsync(actor.EnterpriseId, id, ct)
            ?? throw new SecurityFailure(404, "Media.NotFound", "Logo não encontrado.");

        if (await assets.IsReferencedAsync(actor.EnterpriseId, id, ct))
            throw new SecurityFailure(409, "Media.InUse", "O logo é referenciado por edição/revisão preservada ou possui retenção de uso no DROP.");

        using var media = writes.Begin(actor.EnterpriseId);
        asset.Delete();
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
