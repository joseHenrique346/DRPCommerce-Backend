using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    /// <summary>
    /// O carrinho é gravado pelo próprio cliente na vitrine. Com o escopo aberto, apenas o carrinho do cliente autenticado
    /// na loja atual pode mudar; sem o escopo, o carrinho não pode ser alterado por nenhum outro fluxo.
    /// </summary>
    private bool GuardCartWrites(List<EntityEntry> changes)
    {
        if (_cartWrites.EnterpriseId is not long tenant || _cartWrites.CustomerId is not long customer)
        {
            if (changes.Any(entry => entry.Entity is Cart or CartItem))
                throw new SecurityFailure(403, "Cart.UseWorkflow", "Use os fluxos do carrinho.");

            return false;
        }

        if (tenant != _tenantProvider.GetEnterpriseId())
            throw new SecurityFailure(403, "Security.TenantMismatch", "Carrinho não pertence à loja atual.");

        // Carrinhos rastreados que pertencem ao cliente do escopo; itens só podem ser gravados nesses carrinhos.
        var ownedCarts = ChangeTracker.Entries<Cart>()
            .Where(entry => entry.Entity.EnterpriseId == tenant && entry.Entity.CustomerId == customer)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var entry in changes)
        {
            var allowed = entry.Entity switch
            {
                Cart cart => ownedCarts.Contains(cart) && entry.State != EntityState.Deleted,
                CartItem item => item.EnterpriseId == tenant &&
                    ownedCarts.Any(cart => ReferenceEquals(cart, item.Cart) || (item.CartId > 0 && cart.Id == item.CartId)),
                _ => throw new SecurityFailure(403, "Cart.InvalidScope", "Gravação fora do escopo do carrinho.")
            };

            if (!allowed)
                throw new SecurityFailure(403, "Cart.InvalidScope", "O carrinho não pertence ao cliente autenticado.");

            if (entry.State == EntityState.Modified &&
                entry.Properties.Any(property => property.IsModified && property.Metadata.Name is
                    nameof(Cart.EnterpriseId) or nameof(Cart.CustomerId) or nameof(CartItem.CartId) or
                    nameof(CartItem.ProductId) or nameof(CartItem.ProductVariantId)))
                throw new SecurityFailure(403, "Cart.InvalidScope", "Os vínculos do carrinho não podem ser alterados.");
        }

        return true;
    }
}
