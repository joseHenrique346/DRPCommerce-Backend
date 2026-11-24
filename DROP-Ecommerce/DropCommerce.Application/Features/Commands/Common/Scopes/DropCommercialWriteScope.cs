namespace DropCommerce.Application.Contracts;

public sealed class DropCommercialWriteScope
{
    public string? Purpose { get; private set; }
    public long? EnterpriseId { get; private set; }

    public IDisposable Begin(string purpose, long? enterpriseId = null)
    {
        if (Purpose is not null || purpose is not ("sync" or "catalog" or "reactivation" or "transfer" or "sales" or "expiration"))
            throw new InvalidOperationException("Escopo comercial DROP inválido.");

        Purpose = purpose;
        EnterpriseId = enterpriseId;

        return new Lease(this);
    }

    private sealed class Lease(DropCommercialWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.Purpose = null;
            owner.EnterpriseId = null;
        }
    }
}
