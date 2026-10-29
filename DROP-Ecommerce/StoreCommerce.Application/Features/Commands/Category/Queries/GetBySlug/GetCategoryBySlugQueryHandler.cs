using MediatR;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCategoryBySlugQueryHandler(CategoryCatalogReader reader)
    : IRequestHandler<GetCategoryBySlugQuery, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var result = await reader.ReadAsync(true, cancellationToken);

        if (result.IsFailure)
            return Result<CategoryResponse>.Failure(result.Errors);

        var slug = request.Slug.Trim().ToLowerInvariant();
        var category = result.Content!.SingleOrDefault(Category => Category.Slug == slug);

        return category is null
            ? Result<CategoryResponse>.NotFound("Category.NotFound", "Categoria não encontrada.")
            : Result<CategoryResponse>.Success(CategoryResponse.FromEntity(category));
    }
}
