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

    /// <summary>
    /// Quantidade e valor total das compras concluídas do cliente (confirmadas até entregues), opcionalmente sem um pedido.
    /// </summary>
    /// <summary>
    /// Carrega o pedido (rastreado) com tudo o que o ciclo de vida altera: itens e SKUs, cupom, reserva de estoque e transações.
    /// Ignora os filtros globais e filtra a loja explicitamente, para funcionar também no worker e no webhook.
    /// </summary>
    Task<Order?> GetForWorkflowAsync(long enterpriseId, long orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Próximo número de pedido da loja (inclui pedidos excluídos, números nunca são reaproveitados).
    /// Deve ser chamado dentro de uma transação com a loja bloqueada (LockCatalogAsync).
    /// </summary>
    Task<long> GetNextOrderNumberAsync(long enterpriseId, CancellationToken cancellationToken);

    Task<(List<Order> Items, int TotalCount)> SearchAsync(Models.OrderSearchCriteria criteria, CancellationToken cancellationToken);

    Task<Order?> FindByIdempotencyKeyAsync(long enterpriseId, long customerId, string idempotencyKey, CancellationToken cancellationToken);

    Task<List<long>> GetTenantsWithExpiredPaymentsAsync(DateTime now, CancellationToken cancellationToken);

    Task<List<long>> GetExpiredPaymentOrderIdsAsync(long enterpriseId, DateTime now, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Localiza o pedido de uma cobrança pela referência do gateway (usado pelo resultado de pagamento/webhook).
    /// </summary>
    Task<(long EnterpriseId, long OrderId)?> FindByPaymentReferenceAsync(string provider, string reference, CancellationToken cancellationToken);

    /// <summary>
    /// Pedido com itens, transações e histórico para exibição (respeita os filtros da loja atual).
    /// </summary>
    Task<Order?> GetDetailAsync(long orderId, long? customerId, CancellationToken cancellationToken);

    Task<(List<Order> Items, int TotalCount)> GetPageForCustomerAsync(long customerId, int page, int pageSize, CancellationToken cancellationToken);

    Task<(int CompletedOrders, decimal TotalSpent)> GetCustomerPurchaseSummaryAsync(long customerId, long? excludeOrderId, CancellationToken cancellationToken = default);
}
