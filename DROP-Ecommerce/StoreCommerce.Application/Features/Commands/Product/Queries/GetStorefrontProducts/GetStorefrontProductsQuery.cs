using MediatR;
using StoreCommerce.Application.Contracts.Store;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetStorefrontProductsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<List<StorefrontProductResponse>>>;
