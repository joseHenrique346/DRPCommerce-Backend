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

    /// <summary>
    /// Quantidade total de unidades do pedido (soma das quantidades dos itens).
    /// </summary>
    public int TotalQuantity => _listOrderItem.Sum(item => item.Quantity);

    /// <summary>
    /// Indica se o pedido pode sair de Pendente para o status informado: cancelar é sempre permitido;
    /// os demais exigem itens e subtotal igual ou maior que o pedido mínimo da loja.
    /// </summary>
    public bool CanLeavePending(long orderStatusId, StoreSettings settings) =>
        orderStatusId == OrderStatus.Pending.Id ||
        orderStatusId == OrderStatus.Cancelled.Id ||
        (_listOrderItem.Count > 0 && settings.MeetsMinOrderValue(SubTotal));

    public void UpdateStatus(long orderStatusId, long orderPaymentStatusId, StoreSettings settings)
    {
        EnsureSettings(settings);

        if (!BaseStaticEntity.GetAll<OrderStatus>().Any(status => status.Id == orderStatusId))
            throw new ArgumentException($"{nameof(OrderStatusId)} is invalid.");

        if (!BaseStaticEntity.GetAll<OrderPaymentStatus>().Any(status => status.Id == orderPaymentStatusId))
            throw new ArgumentException($"{nameof(OrderPaymentStatusId)} is invalid.");

        if (IsEditable && !CanLeavePending(orderStatusId, settings))
            throw new ArgumentException("O pedido precisa ter itens e atingir o valor mínimo da loja para sair de Pendente.");

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
    public void ApplyCoupon(Coupon? coupon, DateTime now, StoreSettings settings)
    {
        EnsureEditable();
        EnsureSettings(settings);

        if (coupon is not null)
        {
            if (coupon.EnterpriseId != EnterpriseId)
                throw new ArgumentException("Cupom não pertence à loja do pedido.");

            if (coupon.Id != CouponId && !coupon.IsUsable(now))
                throw new ArgumentException("Cupom inativo, fora da vigência ou sem usos disponíveis.");
        }

        Coupon = coupon;
        CouponId = coupon?.Id;
        Recalculate(settings);
    }

    public OrderItem AddProductItem(Product product, int quantity, StoreSettings settings)
    {
        BaseValidate.ValidateNotNull(product, nameof(Product));

        if (product.HasVariants)
            throw new ArgumentException("Selecione uma variante do produto.");

        var variant = product.ListProductVariant.Single(Variant => Variant.IsDefault && !Variant.IsDeleted);

        return AddProductItem(product, variant, quantity, settings);
    }

    public OrderItem AddProductItem(Product product, ProductVariant variant, int quantity, StoreSettings settings)
    {
        BaseValidate.ValidateNotNull(product, nameof(Product));
        BaseValidate.ValidateNotNull(variant, nameof(ProductVariant));

        if (product.EnterpriseId != EnterpriseId || product.IsDeleted || !product.IsActive)
            throw new ArgumentException("Produto inexistente, inativo ou de outra loja.");

        if (variant.ProductId != product.Id || variant.EnterpriseId != EnterpriseId || variant.IsDeleted || !variant.IsActive ||
            (product.HasVariants && variant.IsDefault))
            throw new ArgumentException("Variante indisponível ou incompatível com o produto.");

        var existing = _listOrderItem.FirstOrDefault(item => item.ProductVariantId == variant.Id);

        return AddOrMergeItem(existing, () => OrderItem.Create(this, product.Id, null, product.Name, variant.SKU,
            quantity, variant.Price ?? product.Price, variant), quantity, settings);
    }

    public OrderItem AddServiceItem(Service service, int quantity, StoreSettings settings)
    {
        BaseValidate.ValidateNotNull(service, nameof(Service));

        if (service.EnterpriseId != EnterpriseId || service.IsDeleted || !service.IsActive)
            throw new ArgumentException("Serviço inexistente, inativo ou de outra loja.");

        var existing = _listOrderItem.FirstOrDefault(item => item.ServiceId == service.Id);

        return AddOrMergeItem(existing, () => OrderItem.Create(this, null, service.Id, service.Name, string.Empty, quantity, service.Price), quantity, settings);
    }

    public OrderItem UpdateItemQuantity(long orderItemId, int quantity, StoreSettings settings)
    {
        EnsureEditable();
        EnsureSettings(settings);

        var item = GetItem(orderItemId);
        EnsureItemLimit(TotalQuantity - item.Quantity + quantity, settings);
        item.UpdateQuantity(quantity);
        Recalculate(settings);

        return item;
    }

    public OrderItem RemoveItem(long orderItemId, StoreSettings settings)
    {
        EnsureEditable();
        EnsureSettings(settings);

        var item = GetItem(orderItemId);
        _listOrderItem.Remove(item);
        Recalculate(settings);

        return item;
    }

    /// <summary>
    /// Recalcula subtotal, desconto (com rateio entre os itens) e total do pedido.
    /// O frete permanece zerado até a integração com o serviço de envio.
    /// </summary>
    public void Recalculate(StoreSettings settings)
    {
        EnsureSettings(settings);

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

    private OrderItem AddOrMergeItem(OrderItem? existing, Func<OrderItem> factory, int quantity, StoreSettings settings)
    {
        EnsureEditable();
        EnsureSettings(settings);
        BaseValidate.ValidatePositive(quantity, nameof(OrderItem.Quantity));
        EnsureItemLimit(checked(TotalQuantity + quantity), settings);

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

        Recalculate(settings);

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

    private void EnsureSettings(StoreSettings settings)
    {
        BaseValidate.ValidateNotNull(settings, nameof(StoreSettings));

        if (settings.EnterpriseId != EnterpriseId)
            throw new ArgumentException("Configurações não pertencem à loja do pedido.");
    }

    private static void EnsureItemLimit(int totalQuantity, StoreSettings settings)
    {
        if (!settings.IsWithinItemLimit(totalQuantity))
            throw new ArgumentException($"O pedido excede o limite de {settings.MaxItemsPerOrder} itens da loja.");
    }

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
