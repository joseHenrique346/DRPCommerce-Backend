using StoreCommerce.Application.Media;
using MediatR;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Media;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UploadMediaAssetCommandHandler(
    ICurrentIdentity actor,
    IIdentityRepositories identity,
    IIdentityUnitOfWork unitOfWork,
    IIdentityOperationScope identities,
    MediaWriteScope writes,
    IMediaAssetStore assets,
    MediaOwnerWriteAccess access,
    IOptions<SecurityOptions> options)
    : IRequestHandler<UploadMediaAssetCommand, MediaAssetSummary>
{
    public async Task<MediaAssetSummary> Handle(UploadMediaAssetCommand command, CancellationToken ct)
    {
        var content = command.Content;
        actor.RequirePrimaryOwner();
        var png = PngLogoValidator.Validate(content);
        using var operation = identities.Begin();
        await using var transaction = await access.BeginAsync(ct);
        var contract = await identity.Enterprises.GetContractStateAsync(actor.EnterpriseId, ct)
            ?? throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (!EnterpriseEntitlements.Resolve(contract.PlanId, contract.AddOnId).AllowsLogoCustomization)
            throw new SecurityFailure(409, "Media.PlanRequired", "O contrato não permite personalizar o logo.");

        var slug = await identity.Enterprises.FindValueAsync(Enterprise => Enterprise.Id == actor.EnterpriseId,
            Enterprise => Enterprise.StorefrontSlug, ct, ignoreQueryFilters: true);

        if (await assets.FindHashAsync(actor.EnterpriseId, png.Hash, ct) is { } existing)
        {
            await transaction.CommitAsync(ct);

            return Summary(existing, slug);
        }

        if (await assets.BytesAsync(actor.EnterpriseId, ct) + png.Content.Length > 25L * 1024 * 1024)
            throw new SecurityFailure(409, "Media.StorageLimit", "O limite técnico de logos é 25 MiB por loja. Remova uploads sem uso.");

        var asset = MediaAsset.Create(actor.EnterpriseId, png.Content, png.Hash, png.Width, png.Height);
        using var media = writes.Begin(actor.EnterpriseId);
        assets.Add(asset);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return Summary(asset, slug);
    }

    private MediaAssetSummary Summary(MediaAsset asset, string? slug)
    {
        return new MediaAssetSummary(asset.Id, asset.PublicId, asset.Width, asset.Height, asset.ByteLength, asset.CreatedAt,
            asset.IsRetainedByDrop, StorefrontUrls.Media(options.Value.StorefrontApiBaseUrl, slug, asset.PublicId));
    }
}
