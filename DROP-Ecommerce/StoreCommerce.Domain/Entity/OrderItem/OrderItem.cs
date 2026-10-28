using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class OrderItem : BaseEntity
{
    #region Constants
    public const int ItemNameMaxLength = 200;
    public const int SkuMaxLength = 100;
    #endregion

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

    #region Navigation Properties
    public Order Order { get; private set; }
    public Product? Product { get; private set; }
    public Service? Service { get; private set; }
    #endregion

    #endregion

    #region Constructor
    protected OrderItem() { }

    private OrderItem(Order order, long? productId, long? serviceId, string itemName, string sku, int quantity, decimal unitPrice)
    {
        Order = order;
        OrderId = order.Id;
        ProductId = productId;
        ServiceId = serviceId;
        ItemName = itemName;
        SKU = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = 0;
        TotalPrice = GrossAmount;
    }
    #endregion

    #region Functions
    /// <summary>
    /// Valor bruto do item (quantidade x preço unitário), antes do rateio do desconto do pedido.
    /// </summary>
    public decimal GrossAmount => Quantity * UnitPrice;

    // Itens são criados apenas pelo agregado Order (Order.AddProductItem / Order.AddServiceItem),
    // garantindo que nome, SKU e preço sejam copiados do catálogo e que o pedido seja recalculado.
    internal static OrderItem Create(Order order, long? productId, long? serviceId, string itemName, string sku, int quantity, decimal unitPrice)
    {
        BaseValidate.ValidateNotNull(order, nameof(Order));

        if (productId.HasValue == serviceId.HasValue)
            throw new ArgumentException($"Informe apenas um entre {nameof(ProductId)} e {nameof(ServiceId)}.");

        BaseValidate.ValidateNullablePositive(productId, nameof(ProductId));
        BaseValidate.ValidateNullablePositive(serviceId, nameof(ServiceId));
        BaseValidate.ValidateNotNullOrEmpty(itemName, nameof(ItemName));
        BaseValidate.ValidateMaxLength(itemName, ItemNameMaxLength, nameof(ItemName));
        BaseValidate.ValidateMaxLength(sku, SkuMaxLength, nameof(SKU));
        BaseValidate.ValidatePositive(quantity, nameof(Quantity));
        BaseValidate.ValidatePositive(unitPrice, nameof(UnitPrice));

        return new OrderItem(order, productId, serviceId, itemName, sku, quantity, unitPrice);
    }

    internal void UpdateQuantity(int quantity)
    {
        BaseValidate.ValidatePositive(quantity, nameof(Quantity));

        Quantity = quantity;
    }

    internal void ApplyDiscount(decimal discountAmount)
    {
        BaseValidate.ValidateRange(discountAmount, 0, GrossAmount, nameof(DiscountAmount));

        DiscountAmount = discountAmount;
        TotalPrice = GrossAmount - discountAmount;
    }
    #endregion
}
