using MediatR;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateProductVariantCommand(long ProductId, long Id, VariantDetailsRequest Request) : IRequest<Result<ProductVariantResponse>>;
