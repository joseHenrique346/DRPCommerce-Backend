namespace DropCommerce.Domain.Entity;

public sealed class DropStockImportReceipt : BaseEntity
{
    public Guid OperationId { get; private set; }
    public long EnterpriseId { get; private set; }
    public long? CatalogProductId { get; private set; }
    public DropCatalogProduct? CatalogProduct { get; private set; }
    public bool Accepted { get; private set; }
    public string? RejectionCode { get; private set; }

    private DropStockImportReceipt()
    {
    }

    public static DropStockImportReceipt Record(Guid operationId, long tenant, DropCatalogProduct? product, string? rejectionCode)
    {
        return new DropStockImportReceipt
        {
            OperationId = operationId, EnterpriseId = tenant, CatalogProduct = product,
            CatalogProductId = product?.Id, Accepted = product is not null, RejectionCode = rejectionCode
        };
    }
}
