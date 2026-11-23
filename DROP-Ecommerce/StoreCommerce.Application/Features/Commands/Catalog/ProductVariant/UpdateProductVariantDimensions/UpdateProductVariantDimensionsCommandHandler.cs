using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductVariantDimensionsCommandHandler(
    CatalogWriteContext context)
    : CatalogRequestHandler<UpdateProductVariantDimensionsCommand, ProductVariantResponse>
{
    protected override async Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantDimensionsCommand request, CancellationToken ct)
    {
        var productId = request.ProductId;
        var id = request.Id;

        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(productId, ct);
        var variant = product.ListProductVariant.SingleOrDefault(Variant => Variant.Id == id)
            ?? throw new SecurityFailure(404, "ProductVariant.NotFound", "Variante não encontrada neste produto.");

        if (variant.IsDefault)
            throw new SecurityFailure(400, "ProductVariant.DefaultProtected", "Os dados do SKU padrão são gerenciados pelo produto.");

        var previousCombination = variant.CombinationKey;
        variant.UpdateDimensions(request.Request.Weight,
                    request.Request.Height, request.Request.Width, request.Request.Length);

        if (previousCombination != variant.CombinationKey)
            CatalogWriteContext.EnsureEmptyStock([variant]);

        variant.ValidateAgainstProduct();
        ProductVariantOptionPolicy.ValidateOptionNames(product, variant);
        await context.ValidateVariantAsync(variant, variant.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return ProductVariantResponse.FromEntity(variant);
    }
}
