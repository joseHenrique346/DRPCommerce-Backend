using MediatR;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CreateProductVariantCommand(long ProductId, CreateVariantRequest Request) : IRequest<Result<ProductVariantResponse>>;
