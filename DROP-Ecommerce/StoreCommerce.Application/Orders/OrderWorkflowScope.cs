namespace StoreCommerce.Application.Orders;

/// <summary>
/// Libera, somente dentro dos fluxos de checkout e ciclo de vida do pedido, a gravação conjunta de pedido, pagamento,
/// estoque, cupom e carrinho da loja informada. Com CustomerId, apenas pedidos e carrinho desse cliente podem mudar.
/// </summary>
public sealed class OrderWorkflowScope
{
    public long? EnterpriseId { get; private set; }
    public long? CustomerId { get; private set; }

    public IDisposable Begin(long enterpriseId, long? customerId)
    {
        if (EnterpriseId.HasValue || enterpriseId <= 0 || customerId <= 0)
            throw new InvalidOperationException("Escopo de pedido inválido ou já aberto.");

        EnterpriseId = enterpriseId;
        CustomerId = customerId;

        return new Lease(this);
    }

    private sealed class Lease(OrderWorkflowScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.CustomerId = null;
        }
    }
}
