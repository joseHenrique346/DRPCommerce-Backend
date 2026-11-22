using MediatR;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeleteProductVariantCommand(long ProductId, long Id) : IRequest<Result<bool>>;
