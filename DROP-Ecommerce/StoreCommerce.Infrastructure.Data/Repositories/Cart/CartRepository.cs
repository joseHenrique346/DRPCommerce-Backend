using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class CartRepository(StoreCommerceDbContext context) : ICartRepository
{
    public Task<Cart?> GetForCustomerAsync(long enterpriseId, long customerId, bool tracking, CancellationToken cancellationToken)
    {
        // Os filtros globais esconderiam produtos e SKUs excluídos; a loja é filtrada explicitamente na raiz,
        // e os itens só referenciam produtos da mesma loja (validado na inclusão).
        var query = context.Carts
            .IgnoreQueryFilters()
            .Include(cart => cart.ListCartItem).ThenInclude(item => item.Product)
            .Include(cart => cart.ListCartItem).ThenInclude(item => item.ProductVariant)
            .Where(cart => cart.EnterpriseId == enterpriseId && cart.CustomerId == customerId)
            .AsSplitQuery();

        return (tracking ? query : query.AsNoTracking()).SingleOrDefaultAsync(cancellationToken);
    }

    public void Add(Cart cart) => context.Carts.Add(cart);

    public void RemoveItem(CartItem item) => context.CartItems.Remove(item);
}
