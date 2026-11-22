using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ClearCartCommandHandler(CartService cart) : CartRequestHandler<ClearCartCommand>
{
    protected override Task<CartResponse> ExecuteAsync(ClearCartCommand request, CancellationToken ct) => cart.ClearAsync(ct);
}
