namespace StoreCommerce.Application.Inventory;

public sealed class InventoryWriteScope
{
    public long? EnterpriseId { get; private set; }

    public IDisposable Begin(long enterpriseId)
    {
        if (EnterpriseId.HasValue || enterpriseId <= 0)
            throw new InvalidOperationException("Escopo de estoque inválido ou já aberto.");

        EnterpriseId = enterpriseId;

        return new Lease(this);
    }

    private sealed class Lease(InventoryWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
        }
    }
}
