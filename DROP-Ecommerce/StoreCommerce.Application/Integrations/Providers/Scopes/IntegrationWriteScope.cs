namespace StoreCommerce.Application.Integrations;

/// <summary>
/// Autoriza apenas a persistência dos registros de integração da loja indicada em fluxos internos confiáveis.
/// </summary>
public sealed class IntegrationWriteScope
{
    public long? EnterpriseId { get; private set; }

    public IDisposable Begin(long enterpriseId)
    {
        if (enterpriseId <= 0 || EnterpriseId.HasValue)
            throw new InvalidOperationException("Escopo de integração inválido ou já aberto.");

        EnterpriseId = enterpriseId;

        return new Lease(this);
    }

    private sealed class Lease(IntegrationWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
        }
    }
}
