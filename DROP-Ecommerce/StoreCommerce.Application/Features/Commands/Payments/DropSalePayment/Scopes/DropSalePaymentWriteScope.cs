namespace StoreCommerce.Application.Payments;

public sealed class DropSalePaymentWriteScope
{
    public long? EnterpriseId { get; private set; }

    public IDisposable Begin(long tenant)
    {
        if (EnterpriseId.HasValue || tenant < 1)
            throw new InvalidOperationException("Escopo de pagamento DROP inválido.");

        EnterpriseId = tenant;

        return new Lease(this);
    }

    private sealed class Lease(DropSalePaymentWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
        }
    }
}
