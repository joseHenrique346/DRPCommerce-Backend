using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface ICartRepository
{
    /// <summary>
    /// Carrega o carrinho do cliente com itens, produtos e SKUs, inclusive os excluídos/inativos, para que o cliente
    /// veja o que ficou indisponível e decida remover ou aguardar.
    /// </summary>
    Task<Cart?> GetForCustomerAsync(long enterpriseId, long customerId, bool tracking, CancellationToken cancellationToken);

    void Add(Cart cart);
    void RemoveItem(CartItem item);
}
