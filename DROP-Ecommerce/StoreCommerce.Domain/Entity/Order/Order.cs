using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Entity;

public class Order : BaseEntity, ISoftDeletable, ITenantEntity
{
    #region Constants
    public const int ShippingAddressLineMaxLength = 200;
    public const int ShippingCityMaxLength = 100;
    public const int ShippingZipCodeMaxLength = 20;
    public const int NotesMaxLength = 500;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public long CustomerId { get; private set; }
    public long? CouponId { get; private set; }
    public long OrderStatusId { get; private set; }
    public long OrderPaymentStatusId { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string ShippingAddressLine { get; private set; }
    public string ShippingCity { get; private set; }
    public long ShippingStateId { get; private set; }
    public string ShippingZipCode { get; private set; }
    public string Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public Customer Customer { get; private set; }
    public Coupon? Coupon { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public OrderPaymentStatus OrderPaymentStatus { get; private set; }
    public State ShippingState { get; private set; }
    private readonly List<Invoice> _listInvoice = [];
    public IReadOnlyCollection<Invoice> ListInvoice => _listInvoice.AsReadOnly();
    private readonly List<OrderItem> _listOrderItem = [];
    public IReadOnlyCollection<OrderItem> ListOrderItem => _listOrderItem.AsReadOnly();
    private readonly List<Shipment> _listShipment = [];
    public IReadOnlyCollection<Shipment> ListShipment => _listShipment.AsReadOnly();
    private readonly List<Transaction> _listTransaction = [];
    public IReadOnlyCollection<Transaction> ListTransaction => _listTransaction.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Order() { }

    private Order(long enterpriseId, long customerId, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        EnterpriseId = enterpriseId;
        CustomerId = customerId;
        OrderStatusId = OrderStatus.Pending.Id;
        OrderPaymentStatusId = OrderPaymentStatus.Pending.Id;
        SubTotal = 0;
        DiscountAmount = 0;
        ShippingCost = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingStateId = shippingStateId;
        ShippingZipCode = shippingZipCode;
        Notes = notes;
    }
    #endregion

    #region Functions
    /// <summary>
    /// Cria um pedido vazio, pendente e com valores zerados. Os valores são calculados a partir dos itens.
    /// </summary>
    public static Order Create(long enterpriseId, long customerId, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        ValidateShippingAddress(shippingAddressLine, shippingCity, shippingStateId, shippingZipCode, notes);

        return new Order(enterpriseId, customerId, shippingAddressLine, shippingCity, shippingStateId, shippingZipCode, notes);
    }

    public bool IsEditable => OrderStatusId == OrderStatus.Pending.Id;

    public void UpdateStatus(long orderStatusId, long orderPaymentStatusId)
    {
        if (!BaseStaticEntity.GetAll<OrderStatus>().Any(status => status.Id == orderStatusId))
            throw new ArgumentException($"{nameof(OrderStatusId)} is invalid.");

        if (!BaseStaticEntity.GetAll<OrderPaymentStatus>().Any(status => status.Id == orderPaymentStatusId))
            throw new ArgumentException($"{nameof(OrderPaymentStatusId)} is invalid.");

        OrderStatusId = orderStatusId;
        OrderPaymentStatusId = orderPaymentStatusId;
    }

    public void UpdateShippingAddress(string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        ValidateShippingAddress(shippingAddressLine, shippingCity, shippingStateId, shippingZipCode, notes);

        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingStateId = shippingStateId;
        ShippingZipCode = shippingZipCode;
        Notes = notes;
    }

    /// <summary>
    /// Aplica (ou remove, quando nulo) o cupom do pedido e recalcula os valores.
    /// </summary>
    public void ApplyCoupon(Coupon? coupon, DateTime now)
    {
        EnsureEditable();

        if (coupon is not null)
        {
            if (coupon.EnterpriseId != EnterpriseId)
                throw new ArgumentException("Cupom não pertence à loja do pedido.");

            if (coupon.Id != CouponId && !coupon.IsUsable(now))
                throw new ArgumentException("Cupom inativo, fora da vigência ou sem usos disponíveis.");
        }

        Coupon = coupon;
        CouponId = coupon?.Id;
        Recalculate();
    }

    public OrderItem AddProductItem(Product product, int quantity)
    {
        BaseValidate.ValidateNotNull(product, nameof(Product));

        if (product.EnterpriseId != EnterpriseId || product.IsDeleted || !product.IsActive)
            throw new ArgumentException("Produto inexistente, inativo ou de outra loja.");

        var existing = _listOrderItem.FirstOrDefault(item => item.ProductId == product.Id);

        return AddOrMergeItem(existing, () => OrderItem.Create(this, product.Id, null, product.Name, product.SKU, quantity, product.Price), quantity);
    }

    public OrderItem AddServiceItem(Service service, int quantity)
    {
        BaseValidate.ValidateNotNull(service, nameof(Service));

        if (service.EnterpriseId != EnterpriseId || service.IsDeleted || !service.IsActive)
            throw new ArgumentException("Serviço inexistente, inativo ou de outra loja.");

        var existing = _listOrderItem.FirstOrDefault(item => item.ServiceId == service.Id);

        return AddOrMergeItem(existing, () => OrderItem.Create(this, null, service.Id, service.Name, string.Empty, quantity, service.Price), quantity);
    }

    public OrderItem UpdateItemQuantity(long orderItemId, int quantity)
    {
        EnsureEditable();

        var item = GetItem(orderItemId);
        item.UpdateQuantity(quantity);
        Recalculate();

        return item;
    }

    public OrderItem RemoveItem(long orderItemId)
    {
        EnsureEditable();

        var item = GetItem(orderItemId);
        _listOrderItem.Remove(item);
        Recalculate();

        return item;
    }

    /// <summary>
    /// Recalcula subtotal, desconto (com rateio entre os itens) e total do pedido.
    /// </summary>
    public void Recalculate()
    {
        if (CouponId.HasValue && Coupon is null)
            throw new InvalidOperationException("O cupom do pedido precisa estar carregado para recalcular os valores.");

        SubTotal = _listOrderItem.Sum(item => item.GrossAmount);
        DiscountAmount = Coupon?.CalculateDiscount(SubTotal) ?? 0;
        AllocateDiscount();
        TotalAmount = SubTotal - DiscountAmount + ShippingCost + TaxAmount;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private OrderItem AddOrMergeItem(OrderItem? existing, Func<OrderItem> factory, int quantity)
    {
        EnsureEditable();
        BaseValidate.ValidatePositive(quantity, nameof(OrderItem.Quantity));

        OrderItem item;

        if (existing is not null)
        {
            existing.UpdateQuantity(checked(existing.Quantity + quantity));
            item = existing;
        }
        else
        {
            item = factory();
            _listOrderItem.Add(item);
        }

        Recalculate();

        return item;
    }

    // Distribui o desconto proporcionalmente ao valor bruto de cada item; o último recebe o resíduo do arredondamento.
    private void AllocateDiscount()
    {
        var remaining = DiscountAmount;
        var items = _listOrderItem.OrderBy(item => item.GrossAmount).ToList();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var share = index == items.Count - 1 || SubTotal == 0
                ? remaining
                : Math.Round(DiscountAmount * item.GrossAmount / SubTotal, 2, MidpointRounding.ToZero);

            share = Math.Min(share, item.GrossAmount);
            item.ApplyDiscount(share);
            remaining -= share;
        }
    }

    private OrderItem GetItem(long orderItemId) =>
        _listOrderItem.FirstOrDefault(item => item.Id == orderItemId)
            ?? throw new ArgumentException("Item não pertence ao pedido.");

    private void EnsureEditable()
    {
        if (!IsEditable)
            throw new ArgumentException("Somente pedidos pendentes podem ter itens ou cupom alterados.");
    }

    private static void ValidateShippingAddress(string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        BaseValidate.ValidateNotNullOrEmpty(shippingAddressLine, nameof(ShippingAddressLine));
        BaseValidate.ValidateMaxLength(shippingAddressLine, ShippingAddressLineMaxLength, nameof(ShippingAddressLine));
        BaseValidate.ValidateNotNullOrEmpty(shippingCity, nameof(ShippingCity));
        BaseValidate.ValidateMaxLength(shippingCity, ShippingCityMaxLength, nameof(ShippingCity));
        BaseValidate.ValidatePositive(shippingStateId, nameof(ShippingStateId));
        BaseValidate.ValidateNotNullOrEmpty(shippingZipCode, nameof(ShippingZipCode));
        BaseValidate.ValidateMaxLength(shippingZipCode, ShippingZipCodeMaxLength, nameof(ShippingZipCode));
        BaseValidate.ValidateMaxLength(notes, NotesMaxLength, nameof(Notes));
    }
    #endregion
}
