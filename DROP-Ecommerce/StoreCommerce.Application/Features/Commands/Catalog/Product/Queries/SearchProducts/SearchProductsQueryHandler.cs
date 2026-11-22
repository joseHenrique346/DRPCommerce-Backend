using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Contracts.Store;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchProductsQueryHandler(ProductSearchReader reader) : CatalogRequestHandler<SearchProductsQuery, PagedResponse<ProductResponse>>
{
    protected override async Task<PagedResponse<ProductResponse>> ExecuteAsync(SearchProductsQuery request, CancellationToken ct)
    {
        var (items, count) = await reader.SearchAsync(request.Request, false, ct);

        return new(items.Select(ProductResponse.FromEntity).ToList(), count, request.Request.Page, request.Request.PageSize);
    }
}
