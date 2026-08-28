using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class OrderItem : BaseEntity
{
    #region Properties
    public long OrderId { get; private set; }
    public long? ProductId { get; private set; }
    public long? ServiceId { get; private set; }
    public string ItemName { get; private set; }
    public string SKU { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalPrice { get; private set; }
    #endregion

    #region Constructor
    protected OrderItem() { }

    private OrderItem(long orderId, long? productId, long? serviceId, string itemName, string sku, int quantity, decimal unitPrice, decimal discountAmount, decimal totalPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        ServiceId = serviceId;
        ItemName = itemName;
        SKU = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TotalPrice = totalPrice;
    }
    #endregion

    #region Functions
    public static OrderItem Create(long orderId, long? productId, long? serviceId, string itemName, string sku, int quantity, decimal unitPrice, decimal discountAmount, decimal totalPrice)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidateNullablePositive(productId, nameof(ProductId));
        BaseValidate.ValidateNullablePositive(serviceId, nameof(ServiceId));
        BaseValidate.ValidateNotNullOrEmpty(itemName, nameof(ItemName));
        BaseValidate.ValidateMaxLength(itemName, 255, nameof(ItemName));
        BaseValidate.ValidateMaxLength(sku, 100, nameof(SKU));
        BaseValidate.ValidatePositive(quantity, nameof(Quantity));
        BaseValidate.ValidatePositive(unitPrice, nameof(UnitPrice));
        BaseValidate.ValidatePositiveOrZero(discountAmount, nameof(DiscountAmount));
        BaseValidate.ValidatePositive(totalPrice, nameof(TotalPrice));

        return new OrderItem(orderId, productId, serviceId, itemName, sku, quantity, unitPrice, discountAmount, totalPrice);
    }

    public void UpdateDetails(string itemName, string sku, int quantity, decimal unitPrice, decimal discountAmount, decimal totalPrice)
    {
        BaseValidate.ValidateNotNullOrEmpty(itemName, nameof(ItemName));
        BaseValidate.ValidateMaxLength(itemName, 255, nameof(ItemName));
        BaseValidate.ValidateMaxLength(sku, 100, nameof(SKU));
        BaseValidate.ValidatePositive(quantity, nameof(Quantity));
        BaseValidate.ValidatePositive(unitPrice, nameof(UnitPrice));
        BaseValidate.ValidatePositiveOrZero(discountAmount, nameof(DiscountAmount));
        BaseValidate.ValidatePositive(totalPrice, nameof(TotalPrice));

        ItemName = itemName;
        SKU = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TotalPrice = totalPrice;
    }
    #endregion
}
