namespace StoreCommerce.Application.Carts;

/// <summary>
/// Libera, somente durante os casos de uso de endereços da vitrine, a gravação dos endereços do cliente autenticado.
/// </summary>
public sealed class CustomerAddressWriteScope
{
    public long? EnterpriseId { get; private set; }
    public long? CustomerId { get; private set; }

    public IDisposable Begin(long enterpriseId, long customerId)
    {
        if (EnterpriseId.HasValue || enterpriseId <= 0 || customerId <= 0)
            throw new InvalidOperationException("Escopo de endereços inválido ou já aberto.");

        EnterpriseId = enterpriseId;
        CustomerId = customerId;

        return new Lease(this);
    }

    private sealed class Lease(CustomerAddressWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.CustomerId = null;
        }
    }
}
