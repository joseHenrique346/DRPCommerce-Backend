using MediatR;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCategoryTreeQueryHandler(CategoryCatalogReader reader)
    : IRequestHandler<GetCategoryTreeQuery, Result<List<CategoryTreeResponse>>>
{
    public async Task<Result<List<CategoryTreeResponse>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var result = await reader.ReadAsync(request.Storefront, cancellationToken);

        if (result.IsFailure)
            return Result<List<CategoryTreeResponse>>.Failure(result.Errors);

        var categories = result.Content!;
        var error = CategoryRules.ValidateTopology(categories.ToDictionary(Category => Category.Id));

        if (error is not null)
            return Result<List<CategoryTreeResponse>>.Failure(error);

        var ordered = categories.OrderBy(Category => Category.DisplayOrder)
            .ThenBy(Category => Category.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(Category => Category.Id)
            .ToList();
        var nodes = ordered.ToDictionary(Category => Category.Id, Category => new CategoryTreeResponse(
            Category.Id, Category.ParentCategoryId, Category.Name, Category.Slug, Category.Description,
            Category.ImageUrl, Category.DisplayOrder, Category.IsActive, []));
        var roots = new List<CategoryTreeResponse>();

        foreach (var category in ordered)
        {
            var node = nodes[category.Id];

            if (category.ParentCategoryId is long parentId)
                nodes[parentId].Children.Add(node);
            else
                roots.Add(node);
        }

        return Result<List<CategoryTreeResponse>>.Success(roots);
    }
}
