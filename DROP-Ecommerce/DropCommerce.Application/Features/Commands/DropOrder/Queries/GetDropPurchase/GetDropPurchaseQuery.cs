using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record GetDropPurchaseQuery(long OrderId) : IRequest<Result<DropPurchaseResponse>>;
