using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateCartItemQuantityCommandHandler(CartService cart) : CartRequestHandler<UpdateCartItemQuantityCommand>
{
    protected override Task<CartResponse> ExecuteAsync(UpdateCartItemQuantityCommand request, CancellationToken ct) =>
        cart.UpdateQuantityAsync(request.ItemId, request.Quantity, ct);
}
