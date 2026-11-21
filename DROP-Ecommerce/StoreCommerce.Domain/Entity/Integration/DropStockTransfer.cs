using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public sealed class DropStockTransfer : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long ProductId { get; private set; }
    public Guid OperationId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string SnapshotJson { get; private set; } = string.Empty;
    public long? DropCatalogProductId { get; private set; }
    public Guid? ReturnOperationId { get; private set; }
    public DateTime? ImportedAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }

    private DropStockTransfer()
    {
    }

    public static DropStockTransfer Withdraw(long tenant, long productId, Guid operationId, string key, string snapshot)
    {
        return new DropStockTransfer
        {
            EnterpriseId = tenant, ProductId = productId, OperationId = operationId,
            IdempotencyKey = key, SnapshotJson = snapshot
        };
    }

    public void Imported(long catalogId, DateTime now)
    {
        if (DropCatalogProductId.HasValue && DropCatalogProductId != catalogId)
            throw new InvalidOperationException("Destino de transferência divergente.");

        DropCatalogProductId = catalogId;
        ImportedAt ??= now;
    }

    public void Returned(Guid returnId, DateTime now)
    {
        if (ReturnOperationId.HasValue && ReturnOperationId != returnId)
            throw new InvalidOperationException("Retorno já confirmado por outra operação.");

        ReturnOperationId = returnId;
        ReturnedAt ??= now;
    }
}
