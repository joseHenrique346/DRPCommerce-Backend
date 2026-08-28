namespace DropCommerce.Domain.Entity;

public class DropOrderItem : BaseEntity
{
    #region Properties

    public long DropOrderId { get; private set; }
    public long DropProductId { get; private set; }
    public string ItemName { get; private set; }
    public string SKU { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    #region Navigation Properties

    public DropOrder DropOrder { get; private set; }
    public DropProduct DropProduct { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected DropOrderItem() { }

    private DropOrderItem(long dropOrderId, long dropProductId, string itemName, string sku, int quantity, decimal unitPrice, decimal totalPrice)
    {
        DropOrderId = dropOrderId;
        DropProductId = dropProductId;
        ItemName = itemName;
        SKU = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
    }

    #endregion

    #region Functions

    public static DropOrderItem Create(long dropOrderId, long dropProductId, string itemName, string sku, int quantity, decimal unitPrice, decimal totalPrice)
    {
        BaseValidate.ValidateId(dropOrderId, nameof(dropOrderId));
        BaseValidate.ValidateId(dropProductId, nameof(dropProductId));
        BaseValidate.ValidateString(itemName, nameof(itemName));
        BaseValidate.ValidateString(sku, nameof(sku));
        BaseValidate.ValidateMinimum(quantity, 1, nameof(quantity));
        BaseValidate.ValidateMinimumDecimal(unitPrice, 0.01m, nameof(unitPrice));
        BaseValidate.ValidateMinimumDecimal(totalPrice, 0.01m, nameof(totalPrice));

        return new DropOrderItem(dropOrderId, dropProductId, itemName, sku, quantity, unitPrice, totalPrice);
    }

    public void Update(long dropOrderId, long dropProductId, string itemName, string sku, int quantity, decimal unitPrice, decimal totalPrice)
    {
        BaseValidate.ValidateId(dropOrderId, nameof(dropOrderId));
        BaseValidate.ValidateId(dropProductId, nameof(dropProductId));
        BaseValidate.ValidateString(itemName, nameof(itemName));
        BaseValidate.ValidateString(sku, nameof(sku));
        BaseValidate.ValidateMinimum(quantity, 1, nameof(quantity));
        BaseValidate.ValidateMinimumDecimal(unitPrice, 0.01m, nameof(unitPrice));
        BaseValidate.ValidateMinimumDecimal(totalPrice, 0.01m, nameof(totalPrice));

        DropOrderId = dropOrderId;
        DropProductId = dropProductId;
        ItemName = itemName;
        SKU = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
    }

    #endregion
}
