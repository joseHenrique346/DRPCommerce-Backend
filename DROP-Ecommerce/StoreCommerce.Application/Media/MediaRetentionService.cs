using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Media;

/// <summary>
/// Retenção conservadora antes da validação no DROP; independe de transação distribuída.
/// </summary>
public sealed class MediaRetentionService(IIdentityRepositories identity, IIdentityUnitOfWork unitOfWork,
    IIdentityOperationScope identities, MediaWriteScope writes, IMediaAssetStore assets, IMediaAssetReader reader)
{
    public async Task<MediaAssetReference?> RetainAsync(long tenant, long id, CancellationToken ct)
    {
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
