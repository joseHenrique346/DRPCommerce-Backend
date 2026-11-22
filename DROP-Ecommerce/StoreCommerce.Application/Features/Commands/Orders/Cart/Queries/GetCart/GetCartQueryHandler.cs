using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCartQueryHandler(CartService cart) : CartRequestHandler<GetCartQuery>
{
    protected override Task<CartResponse> ExecuteAsync(GetCartQuery request, CancellationToken ct) => cart.GetAsync(request.CouponCode, ct);
}
