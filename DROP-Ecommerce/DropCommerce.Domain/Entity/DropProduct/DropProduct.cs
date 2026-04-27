namespace DropCommerce.Domain.Entity;

public class DropProduct : BaseEntity
{
    public long DropEventId { get; private set; }
    public long ProductId { get; private set; }
    public string SKU { get; private set; }
    public int UnitsAllocated { get; private set; }
    public int UnitsSold { get; private set; }
    public int MaxPerCustomer { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public DropProduct() { }

    public DropProduct(long dropEventId, long productId, string sku, int unitsAllocated, int unitsSold, int maxPerCustomer, decimal price, bool isActive)
    {
        DropEventId = dropEventId;
        ProductId = productId;
        SKU = sku;
        UnitsAllocated = unitsAllocated;
        UnitsSold = unitsSold;
        MaxPerCustomer = maxPerCustomer;
        Price = price;
        IsActive = isActive;
    }
}
