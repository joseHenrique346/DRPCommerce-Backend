using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RemoveCartItemsCommandHandler(CartService cart) : CartRequestHandler<RemoveCartItemsCommand>
{
    protected override Task<CartResponse> ExecuteAsync(RemoveCartItemsCommand request, CancellationToken ct) =>
        cart.RemoveItemsAsync(request.ItemIds, ct);
}
