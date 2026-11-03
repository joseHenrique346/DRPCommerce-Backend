using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Contracts.Store;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchProductsQueryHandler(ProductSearchReader reader) : CatalogRequestHandler<SearchProductsQuery, PagedResponse<ProductResponse>>
{
    protected override async Task<PagedResponse<ProductResponse>> ExecuteAsync(SearchProductsQuery request, CancellationToken ct)
    {
        var (items, count) = await reader.SearchAsync(request.Request, false, ct);

        return new(items.Select(ProductResponse.FromEntity).ToList(), count, request.Request.Page, request.Request.PageSize);
    }
}

public sealed class SearchStorefrontProductsQueryHandler(ProductSearchReader reader, StoreSettingsReader settings)
    : CatalogRequestHandler<SearchStorefrontProductsQuery, PagedResponse<StorefrontProductResponse>>
{
    protected override async Task<PagedResponse<StorefrontProductResponse>> ExecuteAsync(SearchStorefrontProductsQuery request, CancellationToken ct)
    {
        var (items, count) = await reader.SearchAsync(request.Request, true, ct);
        var policy = await settings.GetCurrentAsync(ct);

        return new(items.Select(Product => StorefrontProductResponse.FromEntity(Product, policy.AllowBackorder)).ToList(),
            count, request.Request.Page, request.Request.PageSize);
    }
}

public sealed class GetProductBySlugQueryHandler(IProductRepository products, CategoryCatalogReader categories, StoreSettingsReader settings)
    : CatalogRequestHandler<GetProductBySlugQuery, StorefrontProductDetailResponse>
{
    protected override async Task<StorefrontProductDetailResponse> ExecuteAsync(GetProductBySlugQuery request, CancellationToken ct)
    {
        var visible = await categories.ReadAsync(true, ct);

        if (visible.IsFailure)
            throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");

        var product = await products.GetWithVariantsBySlugAsync(request.Slug.Trim().ToLowerInvariant(), ct);

        if (product is null || !product.IsActive || !visible.Content!.Any(Category => Category.Id == product.CategoryId))
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
