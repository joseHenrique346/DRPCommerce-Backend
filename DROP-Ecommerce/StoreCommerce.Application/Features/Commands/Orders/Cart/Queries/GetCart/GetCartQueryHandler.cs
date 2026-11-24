using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Domain.Interfaces.Orders;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCartQueryHandler(
    ICartRepository carts,
    CartPurchaseContextReader contexts,
    CartResponseBuilder responses)
    : CartRequestHandler<GetCartQuery>
{
    protected override async Task<CartResponse> ExecuteAsync(GetCartQuery command, CancellationToken ct)
    {
        var context = await contexts.ReadAsync(ct);
        var cart = await carts.GetForCustomerAsync(context.EnterpriseId, context.CustomerId, tracking: false, ct);

        return await responses.BuildAsync(cart, context, command.CouponCode, ct);
    }
}
