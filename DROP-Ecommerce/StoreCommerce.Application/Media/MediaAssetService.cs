using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Media;

public sealed class MediaAssetService(
    ICurrentIdentity actor, IIdentityRepositories identity, IIdentityUnitOfWork unitOfWork,
    IIdentityOperationScope identities, MediaWriteScope writes, IMediaAssetStore assets)
{
    public Task<IReadOnlyList<MediaAssetSummary>> ListAsync(CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        return assets.ListAsync(actor.EnterpriseId, ct);
    }

    public Task<MediaAsset?> FindAsync(long id, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        return assets.FindAsync(actor.EnterpriseId, id, ct);
    }

    public async Task<MediaAssetSummary> UploadAsync(byte[] content, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var png = PngLogoValidator.Validate(content);
        using var operation = identities.Begin();
        await using var transaction = await BeginAsync(ct);
        var contract = await identity.Enterprises.GetContractStateAsync(actor.EnterpriseId, ct)
            ?? throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (!EnterpriseEntitlements.Resolve(contract.PlanId, contract.AddOnId).AllowsLogoCustomization)
            throw new SecurityFailure(409, "Media.PlanRequired", "O contrato não permite personalizar o logo.");

        if (await assets.FindHashAsync(actor.EnterpriseId, png.Hash, ct) is { } existing)
        {
            await transaction.CommitAsync(ct);

            return Summary(existing);
        }

        if (await assets.BytesAsync(actor.EnterpriseId, ct) + png.Content.Length > 25L * 1024 * 1024)
            throw new SecurityFailure(409, "Media.StorageLimit", "O limite técnico de logos é 25 MiB por loja. Remova uploads sem uso.");

        var asset = MediaAsset.Create(actor.EnterpriseId, png.Content, png.Hash, png.Width, png.Height);
        using var media = writes.Begin(actor.EnterpriseId);
        assets.Add(asset);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return Summary(asset);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        using var operation = identities.Begin();
        await using var transaction = await BeginAsync(ct);
        var asset = await assets.FindAsync(actor.EnterpriseId, id, ct)
            ?? throw new SecurityFailure(404, "Media.NotFound", "Logo não encontrado.");

        if (await assets.IsReferencedAsync(actor.EnterpriseId, id, ct))
            throw new SecurityFailure(409, "Media.InUse", "O logo é referenciado por edição/revisão preservada ou possui retenção de uso no DROP.");

        using var media = writes.Begin(actor.EnterpriseId);
        asset.Delete();
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private async Task<IIdentityTransaction> BeginAsync(CancellationToken ct)
    {
        var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            await identity.Users.LockAsync(actor.UserId, ct);
            await identity.Enterprises.LockAsync(actor.EnterpriseId, ct);

            if (!await identity.EnterpriseMemberships.ExistsAsync(Membership =>
                Membership.Id == actor.MembershipId && Membership.UserId == actor.UserId &&
                Membership.EnterpriseId == actor.EnterpriseId && Membership.KindId == 1 && Membership.IsActive &&
                !Membership.IsContractBlocked && Membership.User.IsActive && Membership.Enterprise.IsActive,
                ct, ignoreQueryFilters: true))
                throw new SecurityFailure(403, "Access.PrimaryOwnerRequired", "Apenas o dono principal pode gerir logos.");

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();

            throw;
        }
    }

    private static MediaAssetSummary Summary(MediaAsset asset)
    {
        return new MediaAssetSummary(asset.Id, asset.PublicId, asset.Width, asset.Height, asset.ByteLength, asset.CreatedAt, asset.IsRetainedByDrop);
    }
}
