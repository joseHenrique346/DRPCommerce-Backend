namespace StoreCommerce.Domain.Entity;

public class OrderItem : BaseEntity
{
    public long OrderId { get; private set; }
    public long? ProductId { get; private set; }
    public long? ServiceId { get; private set; }
    public string ItemName { get; private set; }
    public string SKU { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalPrice { get; private set; }

    public OrderItem() { }

    public OrderItem(long orderId, long? productId, long? serviceId, string itemName, string sku, int quantity, decimal unitPrice, decimal discountAmount, decimal totalPrice)
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
}
