using MediatR;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetCategoryTreeQuery(bool Storefront = false) : IRequest<Result<List<CategoryTreeResponse>>>;
