using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Contracts.Store;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SearchProductsQuery(ProductSearchRequest Request) : IRequest<Result<PagedResponse<ProductResponse>>>;
