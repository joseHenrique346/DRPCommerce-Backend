using MediatR;
using StoreCommerce.Application.Contracts.Store;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetStorefrontProductsQueryHandler(CategoryCatalogReader categories, IProductRepository products)
    : IRequestHandler<GetStorefrontProductsQuery, Result<List<StorefrontProductResponse>>>
{
    public async Task<Result<List<StorefrontProductResponse>>> Handle(GetStorefrontProductsQuery request, CancellationToken cancellationToken)
    {
        var visible = await categories.ReadAsync(true, cancellationToken);

        if (visible.IsFailure)
            return Result<List<StorefrontProductResponse>>.Failure(visible.Errors);

        var ids = visible.Content!.Select(Category => Category.Id).ToList();
        var items = await products.GetStorefrontAsync(ids, Math.Clamp(request.Page, 1, 100000),
            Math.Clamp(request.PageSize, 1, 100), cancellationToken);

        return Result<List<StorefrontProductResponse>>.Success(items.Select(Product => new StorefrontProductResponse(
            Product.Id, Product.Name, Product.Slug, Product.Description,
            Product.Price, Product.Brand, Product.ImageUrls, Product.CategoryId)).ToList());
    }
}
