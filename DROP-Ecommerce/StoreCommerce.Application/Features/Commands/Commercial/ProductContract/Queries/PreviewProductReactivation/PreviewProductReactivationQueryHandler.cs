using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PreviewProductReactivationQueryHandler(
    CatalogWriteContext context,
    ICurrentIdentity actor,
    ProductReactivationSelector selectionReader)
    : CatalogRequestHandler<PreviewProductReactivationQuery, ProductReactivationPreview>
{
    protected override async Task<ProductReactivationPreview> ExecuteAsync(PreviewProductReactivationQuery command, CancellationToken ct)
    {
        var request = command.Request;
        actor.RequirePermission(PermissionCodes.ProductsEdit);
        await using var transaction = await context.BeginAsync(ct);
        var selection = await selectionReader.SelectAsync(request, ct);
        await transaction.CommitAsync(ct);

        return new ProductReactivationPreview(context.Tenant, selection.State.Revision, request.Mode,
            selection.Rights.StoreProductLimit, selection.Published,
            Math.Max(0, selection.Rights.StoreProductLimit - selection.Published),
            selection.EligibleCount, selection.Products.Select(ProductReactivationMapping.Candidate).ToList());
    }
}
