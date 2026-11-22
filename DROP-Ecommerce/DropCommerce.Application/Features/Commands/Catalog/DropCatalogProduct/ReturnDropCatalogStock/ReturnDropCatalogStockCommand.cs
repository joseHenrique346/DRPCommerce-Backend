using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ReturnDropCatalogStockCommand(long ProductId) : IRequest<Result<DropStockReturnIntent>>;
