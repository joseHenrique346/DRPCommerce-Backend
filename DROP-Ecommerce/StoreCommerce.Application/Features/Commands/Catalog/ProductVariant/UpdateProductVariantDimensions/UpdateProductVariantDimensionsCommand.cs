using MediatR;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateProductVariantDimensionsCommand(long ProductId, long Id, VariantDimensionsRequest Request) : IRequest<Result<ProductVariantResponse>>;
