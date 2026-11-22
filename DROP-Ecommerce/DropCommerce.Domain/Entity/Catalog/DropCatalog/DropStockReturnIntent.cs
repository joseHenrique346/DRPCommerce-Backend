namespace DropCommerce.Domain.Entity;

public sealed class DropStockReturnIntent : BaseEntity
{
    public Guid OperationId { get; private set; }
    public long EnterpriseId { get; private set; }
    public long CatalogProductId { get; private set; }
    public string SnapshotJson { get; private set; } = string.Empty;
    public DateTime? CompletedAt { get; private set; }

    private DropStockReturnIntent()
    {
    }

    public static DropStockReturnIntent Create(Guid id, long tenant, long catalogId, string snapshot)
    {
        return new DropStockReturnIntent { OperationId = id, EnterpriseId = tenant, CatalogProductId = catalogId, SnapshotJson = snapshot };
    }

    public void Complete(DateTime now)
    {
        CompletedAt ??= now;
    }
}
