namespace StoreCommerce.Application.Contracts;

public sealed class ProductReactivationScope
{
    public long? EnterpriseId { get; private set; }
    public long ExpectedContractRevision { get; private set; }

    public IDisposable Begin(long enterpriseId, long expectedRevision)
    {
        if (enterpriseId <= 0 || expectedRevision <= 0 || EnterpriseId.HasValue)
            throw new InvalidOperationException("Escopo de reativação inválido ou já aberto.");

        EnterpriseId = enterpriseId;
        ExpectedContractRevision = expectedRevision;

        return new Lease(this);
    }

    private sealed class Lease(ProductReactivationScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.ExpectedContractRevision = 0;
        }
    }
}
