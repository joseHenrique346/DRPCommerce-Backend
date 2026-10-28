using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerAsync(long customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(long statusId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega os pedidos (com rastreamento) junto com seus itens e cupom, necessários para recalcular os valores.
    /// </summary>
    Task<List<Order>> GetListWithItemsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default);
}
