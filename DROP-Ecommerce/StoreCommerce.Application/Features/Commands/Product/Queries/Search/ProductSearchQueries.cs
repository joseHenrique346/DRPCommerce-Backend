using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Contracts.Store;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SearchProductsQuery(ProductSearchRequest Request) : IRequest<Result<PagedResponse<ProductResponse>>>;
public sealed record SearchStorefrontProductsQuery(ProductSearchRequest Request) : IRequest<Result<PagedResponse<StorefrontProductResponse>>>;
public sealed record GetProductBySlugQuery(string Slug) : IRequest<Result<StorefrontProductDetailResponse>>;
