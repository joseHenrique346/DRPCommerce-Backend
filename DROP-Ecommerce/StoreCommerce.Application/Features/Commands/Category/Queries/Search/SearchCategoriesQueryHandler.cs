using MediatR;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchCategoriesQueryHandler(ICategoryRepository repository)
    : IRequestHandler<SearchCategoriesQuery, Result<PagedResponse<CategoryResponse>>>
{
    public async Task<Result<PagedResponse<CategoryResponse>>> Handle(SearchCategoriesQuery request, CancellationToken cancellationToken)
    {
        var (items, count) = await repository.SearchAsync(request.Name, request.ParentId, request.IsActive,
            request.RootsOnly, request.Page, request.PageSize, cancellationToken);

        return Result<PagedResponse<CategoryResponse>>.Success(new(
            items.Select(CategoryResponse.FromEntity).ToList(), count, request.Page, request.PageSize));
    }
}
