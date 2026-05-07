namespace DropCommerce.Domain.Entity;

public class DropProduct : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long ProductId { get; private set; }
    public string SKU { get; private set; }
    public int UnitsAllocated { get; private set; }
    public int UnitsSold { get; private set; }
    public int MaxPerCustomer { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    #endregion

    #region Constructors

    protected DropProduct() { }

    private DropProduct(long dropEventId, long productId, string sku, int unitsAllocated, int unitsSold, int maxPerCustomer, decimal price, bool isActive)
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

    #endregion

    #region Functions

    public static DropProduct Create(long dropEventId, long productId, string sku, int unitsAllocated, int unitsSold, int maxPerCustomer, decimal price, bool isActive)
    {
        return new DropProduct(dropEventId, productId, sku, unitsAllocated, unitsSold, maxPerCustomer, price, isActive);
    }

    #endregion
}