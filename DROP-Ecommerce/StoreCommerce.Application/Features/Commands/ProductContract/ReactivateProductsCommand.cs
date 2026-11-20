using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ReactivateProductsCommand(ProductReactivationRequest Request) : IRequest<Result<ProductReactivationResult>>;
