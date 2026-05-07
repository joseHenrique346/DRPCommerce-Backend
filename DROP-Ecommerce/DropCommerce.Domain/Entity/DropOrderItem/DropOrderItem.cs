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
        return new DropOrderItem(dropOrderId, dropProductId, itemName, sku, quantity, unitPrice, totalPrice);
    }

    #endregion
}