using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetCartSelectionCommandHandler(CartService cart) : CartRequestHandler<SetCartSelectionCommand>
{
    protected override Task<CartResponse> ExecuteAsync(SetCartSelectionCommand request, CancellationToken ct) =>
        cart.SetSelectionAsync(request.ItemIds, request.Selected, ct);
}
