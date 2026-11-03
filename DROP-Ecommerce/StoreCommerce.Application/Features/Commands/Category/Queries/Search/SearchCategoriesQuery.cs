using MediatR;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SearchCategoriesQuery(
    string? Name = null, long? ParentId = null, bool? IsActive = null,
    int Page = 1, int PageSize = 20, bool RootsOnly = false) : IRequest<Result<PagedResponse<CategoryResponse>>>;
