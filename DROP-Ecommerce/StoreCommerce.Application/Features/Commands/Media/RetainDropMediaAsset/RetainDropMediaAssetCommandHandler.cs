using StoreCommerce.Application.Media;
using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces.Media;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Media;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RetainDropMediaAssetCommandHandler(
    IIdentityRepositories identity,
    IIdentityUnitOfWork unitOfWork,
    IIdentityOperationScope identities,
    MediaWriteScope writes,
    IMediaAssetStore assets,
    IMediaAssetReader reader)
    : IRequestHandler<RetainDropMediaAssetCommand, MediaAssetReference?>
{
    public async Task<MediaAssetReference?> Handle(RetainDropMediaAssetCommand command, CancellationToken ct)
    {
        var tenant = command.Tenant;
        var id = command.Id;
        using var operation = identities.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await identity.Enterprises.LockAsync(tenant, ct);
        var contract = await identity.Enterprises.GetContractStateAsync(tenant, ct)
            ?? throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (!EnterpriseEntitlements.Resolve(contract.PlanId, contract.AddOnId).AllowsDrop)
            throw new SecurityFailure(409, "Media.DropPlanRequired", "O contrato não permite utilizar logos no DROP.");

        var asset = await assets.FindAsync(tenant, id, ct);

        if (asset is null)
            return null;

        if (!asset.IsRetainedByDrop)
        {
            using var media = writes.Begin(tenant);
            asset.RetainForDrop();
            await unitOfWork.CommitAsync(ct);
        }

        var reference = await reader.GetAsync(tenant, id, ct);
        await transaction.CommitAsync(ct);

        return reference;
    }
}
