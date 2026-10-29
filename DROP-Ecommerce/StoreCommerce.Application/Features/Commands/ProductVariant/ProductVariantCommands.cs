using MediatR;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CreateProductVariantCommand(long ProductId, CreateVariantRequest Request) : IRequest<Result<ProductVariantResponse>>;
public sealed record UpdateProductVariantCommand(long ProductId, long Id, VariantDetailsRequest Request) : IRequest<Result<ProductVariantResponse>>;
public sealed record UpdateProductVariantPricingCommand(long ProductId, long Id, VariantPricingRequest Request) : IRequest<Result<ProductVariantResponse>>;
public sealed record UpdateProductVariantDimensionsCommand(long ProductId, long Id, VariantDimensionsRequest Request) : IRequest<Result<ProductVariantResponse>>;
public sealed record DeleteProductVariantCommand(long ProductId, long Id) : IRequest<Result<bool>>;
public sealed record GetProductVariantsQuery(long ProductId) : IRequest<Result<List<ProductVariantResponse>>>;
