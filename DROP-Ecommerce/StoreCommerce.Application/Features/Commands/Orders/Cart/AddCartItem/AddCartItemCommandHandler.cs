using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AddCartItemCommandHandler(CartService cart) : CartRequestHandler<AddCartItemCommand>
{
    protected override Task<CartResponse> ExecuteAsync(AddCartItemCommand request, CancellationToken ct) =>
        cart.AddItemAsync(request.ProductId, request.ProductVariantId, request.Quantity, ct);
}
