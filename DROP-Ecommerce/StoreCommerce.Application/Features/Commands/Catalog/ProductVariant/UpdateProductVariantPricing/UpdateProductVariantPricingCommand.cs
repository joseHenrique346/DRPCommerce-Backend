using MediatR;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateProductVariantPricingCommand(long ProductId, long Id, VariantPricingRequest Request) : IRequest<Result<ProductVariantResponse>>;
