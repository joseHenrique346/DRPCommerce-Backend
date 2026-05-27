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

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public ICollection<DropOrderItem> ListDropOrderItem { get; private set; } = [];
    public ICollection<DropReservation> ListDropReservation { get; private set; } = [];
    public ICollection<WaitlistEntry> ListWaitlistEntry { get; private set; } = [];

    #endregion

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
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(productId, nameof(productId));
        BaseValidate.ValidateString(sku, nameof(sku));
        BaseValidate.ValidateMinimum(unitsAllocated, 1, nameof(unitsAllocated));
        BaseValidate.ValidatePositive(unitsSold, nameof(unitsSold));
        BaseValidate.ValidateMinimum(maxPerCustomer, 1, nameof(maxPerCustomer));
        BaseValidate.ValidateMinimumDecimal(price, 0.01m, nameof(price));

        return new DropProduct(dropEventId, productId, sku, unitsAllocated, unitsSold, maxPerCustomer, price, isActive);
    }

    #endregion
}
