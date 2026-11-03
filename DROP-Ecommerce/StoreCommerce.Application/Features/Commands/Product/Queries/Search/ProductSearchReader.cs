using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.Models;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductSearchReader(IProductRepository products, CategoryCatalogReader categories)
{
    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductSearchRequest request, bool storefront, CancellationToken ct)
    {
        if (storefront && request.IsActive.HasValue)
            throw new SecurityFailure(400, "Product.PublicActiveFilter", "O filtro isActive é exclusivo do backoffice.");

        var result = await categories.ReadAsync(storefront, ct);

        if (result.IsFailure)
            throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");

        var available = result.Content!;
        IReadOnlyCollection<long>? ids = storefront ? available.Select(Category => Category.Id).ToList() : null;

        if (request.CategoryId is long categoryId)
        {
            var selected = new HashSet<long>();

            if (available.Any(Category => Category.Id == categoryId))
                selected.Add(categoryId);

            if (request.IncludeSubcategories)
            {
                for (var depth = 1; depth < Category.MaximumDepth; depth++)
                {
                    foreach (var category in available.Where(Category => Category.ParentCategoryId.HasValue && selected.Contains(Category.ParentCategoryId.Value)).ToList())
                        selected.Add(category.Id);
                }
            }

            ids = selected;
        }

        return await products.SearchAsync(new ProductSearchCriteria(request.Term, ids, request.Brand, request.MinPrice,
            request.MaxPrice, request.InStock, request.IsActive, request.Sort, request.Page, request.PageSize, storefront), ct);
    }
}
