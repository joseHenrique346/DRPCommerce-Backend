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
    public const int IdempotencyKeyMaxLength = 64;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public long CustomerId { get; private set; }
    /// <summary>
    /// Número sequencial do pedido na loja (1, 2, 3...), exibido ao cliente no lugar do Id.
    /// </summary>
    public long OrderNumber { get; private set; }
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
    public string? IdempotencyKey { get; private set; }
    public long? PaymentMethodId { get; private set; }
    public DateTime? PaymentExpiresAt { get; private set; }
    public bool IsLocked { get; private set; }
    public long? StockReservationId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public Customer Customer { get; private set; }
    public Coupon? Coupon { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public OrderPaymentStatus OrderPaymentStatus { get; private set; }
    public State ShippingState { get; private set; }
    public StockReservation? StockReservation { get; private set; }
    private readonly List<OrderStatusHistory> _listStatusHistory = [];
    public IReadOnlyCollection<OrderStatusHistory> ListStatusHistory => _listStatusHistory.AsReadOnly();
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

    /// <summary>
    /// Cria o pedido do checkout da vitrine. Itens e cupom são incluídos em seguida e o pedido é fechado por LockForPayment.
    /// </summary>
    public static Order CreateCheckout(long enterpriseId, long customerId, string shippingAddressLine, string shippingCity, long shippingStateId,
        string shippingZipCode, string notes, string idempotencyKey, long paymentMethodId, DateTime paymentExpiresAt, DateTime now)
    {
        BaseValidate.ValidateNotNullOrEmpty(idempotencyKey, nameof(IdempotencyKey));
        BaseValidate.ValidateMaxLength(idempotencyKey, IdempotencyKeyMaxLength, nameof(IdempotencyKey));
        BaseValidate.ValidateGreaterThan(paymentExpiresAt, now, nameof(PaymentExpiresAt));

        if (!BaseStaticEntity.GetAll<TransactionMethod>().Any(method => method.Id == paymentMethodId))
            throw new ArgumentException("Forma de pagamento inválida.");

        var order = Create(enterpriseId, customerId, shippingAddressLine, shippingCity, shippingStateId, shippingZipCode, notes);
        order.IdempotencyKey = idempotencyKey.Trim();
        order.PaymentMethodId = paymentMethodId;
        order.PaymentExpiresAt = paymentExpiresAt;
        order.RecordCreation(OrderChangeSource.Customer, customerId, now);

        return order;
    }

    /// <summary>
    /// Define o número do pedido na loja. Chamado uma única vez, com a loja bloqueada para numeração.
    /// </summary>
    public void AssignNumber(long orderNumber)
    {
        BaseValidate.ValidatePositive(orderNumber, nameof(OrderNumber));

        if (OrderNumber != 0)
            throw new InvalidOperationException("O pedido já possui número.");

        OrderNumber = orderNumber;
    }

    public void RecordCreation(OrderChangeSource source, long? actorId, DateTime now)
    {
        if (_listStatusHistory.Count == 0)
            _listStatusHistory.Add(OrderStatusHistory.Create(this, null, null, source, actorId, "Pedido criado", now));
    }

    /// <summary>
    /// Pedido pendente que ainda aceita itens e cupom. Pedidos do checkout ficam fechados enquanto aguardam pagamento,
    /// porque o valor cobrado e o estoque reservado precisam continuar iguais.
    /// </summary>
    public bool IsEditable => OrderStatusId == OrderStatus.Pending.Id && !IsLocked;

    public bool IsAwaitingPayment => OrderStatusId == OrderStatus.Pending.Id && OrderPaymentStatusId == OrderPaymentStatus.Pending.Id;

    public bool IsPaid => OrderPaymentStatusId == OrderPaymentStatus.Paid.Id;

    public bool IsCancelled => OrderStatusId == OrderStatus.Cancelled.Id;

    public bool CanBeCancelled =>
        OrderStatusId == OrderStatus.Pending.Id || OrderStatusId == OrderStatus.Confirmed.Id || OrderStatusId == OrderStatus.Processing.Id;

    public void AttachStockReservation(StockReservation reservation)
    {
        BaseValidate.ValidateNotNull(reservation, nameof(StockReservation));

        if (reservation.EnterpriseId != EnterpriseId)
            throw new ArgumentException("Reserva não pertence à loja do pedido.");

        StockReservation = reservation;
        StockReservationId = reservation.Id;
    }

    /// <summary>
    /// Fecha o pedido do checkout para pagamento: itens, cupom e valores não mudam mais.
    /// </summary>
    public void LockForPayment()
    {
        if (!IsAwaitingPayment || _listOrderItem.Count == 0 || TotalAmount < 0)
            throw new ArgumentException("O pedido precisa estar pendente e ter itens para aguardar pagamento.");

        IsLocked = true;
    }

    /// <summary>
    /// Pagamento aprovado: o pedido passa a Confirmado/Pago.
    /// </summary>
    public void ConfirmPayment(StoreSettings settings, OrderChangeSource source, long? actorId, string? reason, DateTime now)
    {
        EnsureSettings(settings);

        if (!IsAwaitingPayment)
            throw new ArgumentException("Somente pedidos pendentes aguardando pagamento podem ser confirmados.");

        if (_listOrderItem.Count == 0)
            throw new ArgumentException("O pedido não possui itens.");

        if (!settings.MeetsMinOrderValue(SubTotal))
            throw new ArgumentException("O pedido não atinge o valor mínimo da loja.");

        IsLocked = true;
        Transition(OrderStatus.Confirmed.Id, OrderPaymentStatus.Paid.Id, source, actorId, reason ?? "Pagamento confirmado", now);
    }

    public void StartProcessing(OrderChangeSource source, long? actorId, DateTime now) =>
        Advance(OrderStatus.Confirmed.Id, OrderStatus.Processing.Id, source, actorId, "Pedido em separação", now);

    /// <summary>
    /// Em processamento → Enviado. Com shipment, registra o envio (transportadora, rastreio e previsão) no pedido.
    /// </summary>
    public void MarkShipped(Shipment? shipment, OrderChangeSource source, long? actorId, string? reason, DateTime now)
    {
        if (shipment is not null)
        {
            if (shipment.OrderId != Id)
                throw new ArgumentException("O envio não pertence ao pedido.");

            _listShipment.Add(shipment);
        }

        var tracking = shipment is null || string.IsNullOrWhiteSpace(shipment.TrackingCode) ? string.Empty : $" (rastreio {shipment.TrackingCode})";
        Advance(OrderStatus.Processing.Id, OrderStatus.Shipped.Id, source, actorId, (reason ?? "Pedido enviado") + tracking, now);
    }

    public void MarkDelivered(OrderChangeSource source, long? actorId, DateTime now)
    {
        Advance(OrderStatus.Shipped.Id, OrderStatus.Delivered.Id, source, actorId, "Pedido entregue", now);

        foreach (var shipment in _listShipment)
            shipment.MarkDelivered(now);
    }

    /// <summary>
    /// Cancela pedidos pendentes, confirmados ou em separação. A devolução de estoque, cupom e pagamento é feita pelo fluxo do pedido.
    /// </summary>
    public void Cancel(string reason, OrderChangeSource source, long? actorId, DateTime now)
    {
        BaseValidate.ValidateNotNullOrEmpty(reason, "Motivo");

        if (!CanBeCancelled)
            throw new ArgumentException("Somente pedidos pendentes, confirmados ou em separação podem ser cancelados.");

        IsLocked = true;
        Transition(OrderStatus.Cancelled.Id, OrderPaymentStatusId, source, actorId, reason, now);
    }

    /// <summary>
    /// Pagamento recusado ou cancelado no gateway: o pedido pendente é cancelado.
    /// </summary>
    public void MarkPaymentFailed(string reason, OrderChangeSource source, long? actorId, DateTime now)
    {
        if (!IsAwaitingPayment)
            throw new ArgumentException("Somente pedidos aguardando pagamento podem ter o pagamento recusado.");

        IsLocked = true;
        Transition(OrderStatus.Cancelled.Id, OrderPaymentStatus.Failed.Id, source, actorId, reason, now);
    }

    /// <summary>
    /// Registra o estorno total do pagamento de um pedido cancelado (inclusive pagamento aprovado depois do cancelamento).
    /// </summary>
    public void RegisterFullRefund(OrderChangeSource source, long? actorId, string reason, DateTime now)
    {
        if (OrderPaymentStatusId == OrderPaymentStatus.FullRefund.Id)
            return;

        if (!IsCancelled)
            throw new ArgumentException("Somente pedidos cancelados têm o pagamento estornado por este fluxo.");

        Transition(OrderStatusId, OrderPaymentStatus.FullRefund.Id, source, actorId, reason, now);
    }

    /// <summary>
    /// Status em que o pedido conta como compra concluída no histórico do cliente (condições de cupom).
    /// </summary>
    public static IReadOnlyList<long> CompletedStatusIds { get; } =
        [OrderStatus.Confirmed.Id, OrderStatus.Processing.Id, OrderStatus.Shipped.Id, OrderStatus.Delivered.Id];

    /// <summary>
    /// Quantidade total de unidades do pedido (soma das quantidades dos itens).
    /// </summary>
    public int TotalQuantity => _listOrderItem.Sum(item => item.Quantity);

    /// <summary>
    /// O endereço de entrega só pode mudar até o pedido ser enviado (pendente, confirmado ou em processamento).
    /// </summary>
    public bool CanChangeShippingAddress =>
        OrderStatusId == OrderStatus.Pending.Id || OrderStatusId == OrderStatus.Confirmed.Id || OrderStatusId == OrderStatus.Processing.Id;

    public bool IsShippingAddressDifferent(string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode) =>
        ShippingAddressLine != shippingAddressLine || ShippingCity != shippingCity || ShippingStateId != shippingStateId || ShippingZipCode != shippingZipCode;

    public void UpdateShippingAddress(string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        ValidateShippingAddress(shippingAddressLine, shippingCity, shippingStateId, shippingZipCode, notes);

        if (!CanChangeShippingAddress && IsShippingAddressDifferent(shippingAddressLine, shippingCity, shippingStateId, shippingZipCode))
            throw new ArgumentException("O endereço de entrega não pode ser alterado depois que o pedido foi enviado.");

        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingStateId = shippingStateId;
        ShippingZipCode = shippingZipCode;
        Notes = notes;
    }

    /// <summary>
    /// Aplica (ou remove, quando nulo) o cupom do pedido e recalcula os valores.
    /// Disponibilidade e condições do cliente são exigidas na aplicação; as condições do pedido (valor e quantidade)
    /// apenas zeram o desconto enquanto não forem atendidas e são exigidas quando o pedido sai de Pendente.
    /// </summary>
    public void ApplyCoupon(Coupon? coupon, CouponCustomerHistory? history, DateTime now, StoreSettings settings)
    {
        EnsureEditable();
        EnsureSettings(settings);

        if (coupon is not null)
        {
            if (coupon.EnterpriseId != EnterpriseId)
                throw new ArgumentException("Cupom não pertence à loja do pedido.");

            var failure = coupon.Id == CouponId ? null : coupon.CheckAvailability(now, history) ?? coupon.CheckCustomerConditions(history);

            if (failure is not null)
                throw new ArgumentException(failure.Message);
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
        DiscountAmount = Coupon?.CalculateDiscount(SubTotal, TotalQuantity) ?? 0;
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

    private void Advance(long expectedStatusId, long nextStatusId, OrderChangeSource source, long? actorId, string reason, DateTime now)
    {
        if (OrderStatusId != expectedStatusId)
        {
            var expected = BaseStaticEntity.GetAll<OrderStatus>().First(status => status.Id == expectedStatusId).Description;
            throw new ArgumentException($"Esta etapa exige o pedido no status {expected}.");
        }

        Transition(nextStatusId, OrderPaymentStatusId, source, actorId, reason, now);
    }

    private void Transition(long statusId, long paymentStatusId, OrderChangeSource source, long? actorId, string reason, DateTime now)
    {
        var fromStatus = OrderStatusId;
        var fromPayment = OrderPaymentStatusId;

        OrderStatusId = statusId;
        OrderPaymentStatusId = paymentStatusId;
        _listStatusHistory.Add(OrderStatusHistory.Create(this, fromStatus, fromPayment, source, actorId, reason, now));
    }

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
            throw new ArgumentException("Somente pedidos pendentes que ainda não foram enviados para pagamento podem ter itens ou cupom alterados.");
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
