using StoreCommerce.Domain.Interfaces.Catalog;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Contracts.Store;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetProductBySlugQueryHandler(IProductRepository products, CategoryCatalogReader categories, StoreSettingsReader settings)
    : CatalogRequestHandler<GetProductBySlugQuery, StorefrontProductDetailResponse>
{
    protected override async Task<StorefrontProductDetailResponse> ExecuteAsync(GetProductBySlugQuery request, CancellationToken ct)
    {
        var visible = await categories.ReadAsync(true, ct);

        if (visible.IsFailure)
            throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");

        var product = await products.GetWithVariantsBySlugAsync(request.Slug.Trim().ToLowerInvariant(), ct);

        if (product is null || !product.IsActive || product.IsContractBlocked || !visible.Content!.Any(Category => Category.Id == product.CategoryId))
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado.");

        var policy = await settings.GetCurrentAsync(ct);
        var variants = product.ListProductVariant.Where(Variant => Variant.IsActive && !Variant.IsDeleted &&
            (!product.HasVariants || !Variant.IsDefault)).OrderBy(Variant => Variant.Id).ToList();

        if (variants.Count == 0)
            throw new SecurityFailure(404, "Product.NotFound", "Produto sem SKU disponível.");

        return new(StorefrontProductResponse.FromEntity(product, policy.AllowBackorder),
            variants.Select(Variant => StorefrontVariantResponse.FromEntity(Variant, policy.AllowBackorder)).ToList());
    }
}
