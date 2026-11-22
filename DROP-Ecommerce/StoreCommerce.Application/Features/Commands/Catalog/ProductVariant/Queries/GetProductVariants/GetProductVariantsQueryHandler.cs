using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetProductVariantsQueryHandler(CatalogWriteContext context) : CatalogRequestHandler<GetProductVariantsQuery, List<ProductVariantResponse>>
{
    protected override async Task<List<ProductVariantResponse>> ExecuteAsync(GetProductVariantsQuery request, CancellationToken ct)
    {
        var product = await context.GetProductAsync(request.ProductId, ct);

        return product.ListProductVariant.OrderByDescending(Variant => Variant.IsDefault).ThenBy(Variant => Variant.Id)
            .Select(ProductVariantResponse.FromEntity).ToList();
    }
}
