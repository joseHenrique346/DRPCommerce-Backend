namespace DropCommerce.Application.Integrations.Store.Interfaces;

/// <summary>
/// Transporta o tenant de uma operação interna pelo fluxo assíncrono, sem aceitar cabeçalhos públicos.
/// </summary>
public sealed class StoreIntegrationScope
{
    private readonly AsyncLocal<long?> enterprise = new();

    public long? EnterpriseId => enterprise.Value;

    public IDisposable Begin(long enterpriseId)
    {
        if (enterpriseId < 1)
            throw new ArgumentOutOfRangeException(nameof(enterpriseId));

        var previous = enterprise.Value;
        enterprise.Value = enterpriseId;

        return new Lease(this, previous);
    }

    private sealed class Lease(StoreIntegrationScope scope, long? previous) : IDisposable
    {
        public void Dispose()
        {
            scope.enterprise.Value = previous;
        }
    }
}
