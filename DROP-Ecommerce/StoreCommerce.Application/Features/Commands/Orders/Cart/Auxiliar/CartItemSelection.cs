using CartEntity = StoreCommerce.Domain.Entity.Cart;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Features.Commands;

public static class CartItemSelection
{
    public static void EnsureItem(CartEntity cart, long itemId)
    {
        if (!cart.ContainsItem(itemId))
            throw new SecurityFailure(404, "CartItem.NotFound", $"Item {itemId} não encontrado no carrinho.");
    }
}
