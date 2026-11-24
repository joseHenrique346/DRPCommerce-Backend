using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateCartItemQuantityCommand(long ItemId, int Quantity) : IRequest<Result<CartResponse>>;
