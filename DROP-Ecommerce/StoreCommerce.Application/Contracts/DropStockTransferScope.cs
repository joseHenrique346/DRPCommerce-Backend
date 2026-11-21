namespace StoreCommerce.Application.Contracts;

public sealed class DropStockTransferScope
{
    public long? EnterpriseId { get; private set; }
    public long? ProductId { get; private set; }

    public IDisposable Begin(long tenant, long productId)
    {
        if (EnterpriseId.HasValue || tenant < 1 || productId < 1)
            throw new InvalidOperationException("Escopo de transferência inválido.");

        EnterpriseId = tenant;
        ProductId = productId;

        return new Lease(this);
    }

    private sealed class Lease(DropStockTransferScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.ProductId = null;
        }
    }
}
