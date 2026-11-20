namespace StoreCommerce.Application.Media;

public sealed class MediaWriteScope
{
    public long? EnterpriseId { get; private set; }

    public IDisposable Begin(long tenant)
    {
        if (tenant <= 0 || EnterpriseId.HasValue)
            throw new InvalidOperationException("Escopo de mídia inválido.");

        EnterpriseId = tenant;

        return new Lease(this);
    }

    private sealed class Lease(MediaWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
        }
    }
}
