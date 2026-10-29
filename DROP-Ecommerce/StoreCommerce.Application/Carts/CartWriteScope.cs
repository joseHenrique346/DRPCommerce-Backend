namespace StoreCommerce.Application.Carts;

/// <summary>
/// Libera, somente durante os casos de uso do carrinho, a gravação do carrinho do cliente autenticado.
/// Fora desse escopo o carrinho não pode ser alterado; dentro dele nenhuma outra entidade pode.
/// </summary>
public sealed class CartWriteScope
{
    public long? EnterpriseId { get; private set; }
    public long? CustomerId { get; private set; }

    public IDisposable Begin(long enterpriseId, long customerId)
    {
        if (EnterpriseId.HasValue || enterpriseId <= 0 || customerId <= 0)
            throw new InvalidOperationException("Escopo de carrinho inválido ou já aberto.");

        EnterpriseId = enterpriseId;
        CustomerId = customerId;

        return new Lease(this);
    }

    private sealed class Lease(CartWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.CustomerId = null;
        }
    }
}
