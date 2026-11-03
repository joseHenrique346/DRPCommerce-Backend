using MediatR;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetCategoryBySlugQuery(string Slug) : IRequest<Result<CategoryResponse>>;
