using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Contracts.Store;

namespace StoreCommerce.Application.Features.Commands;

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
