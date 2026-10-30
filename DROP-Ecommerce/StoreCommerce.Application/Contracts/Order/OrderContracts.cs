using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Finaliza a compra dos itens selecionados do carrinho. IdempotencyKey deve ser gerada pelo front a cada tentativa de compra
/// (ex.: um GUID): repetir a mesma chave devolve o mesmo pedido em vez de criar outro.
/// Endereço de entrega, em ordem de prioridade: AddressId (endereço salvo do cliente); UseProfileAddress (endereço padrão
/// salvo ou, se não houver, o do cadastro); ou os campos de endereço informados.
/// </summary>
public sealed record CheckoutRequest(
    string IdempotencyKey,
    long PaymentMethodId,
    string? CouponCode,
    bool UseProfileAddress,
    string? ShippingAddressLine,
    string? ShippingCity,
    long? ShippingStateId,
    string? ShippingZipCode,
    string? Notes,
    long? AddressId = null);

public sealed record CancelOrderRequest(string Reason);

/// <summary>
/// Confirmação manual de pagamento recebido. Sem integração com gateway, o backoffice registra aqui o pagamento.
/// TransactionMethodId só é exigido em pedidos criados pelo backoffice (sem cobrança registrada).
/// </summary>
public sealed record ConfirmPaymentRequest(long? TransactionMethodId, string? GatewayReference, string? Note);

/// <summary>
/// Dados do envio. Obrigatórios em pedidos com produtos físicos: ShipmentTypeId, EstimatedDelivery e CarrierName
/// (dispensado na retirada no local).
/// </summary>
public sealed record ShipOrderRequest(long? ShipmentTypeId, string? CarrierName, string? TrackingCode, DateTime? EstimatedDelivery, string? Note);

public sealed record ShipmentInfoResponse(
    long Id, long ShipmentTypeId, string ShipmentType, long StatusId, string Status, string CarrierName, string TrackingCode,
    DateTime EstimatedDelivery, DateTime? ShippedAt, DateTime? DeliveredAt)
{
    public static ShipmentInfoResponse FromEntity(Shipment shipment) => new(
        shipment.Id, shipment.ShipmentTypeId,
        BaseStaticEntity.GetAll<ShipmentType>().First(type => type.Id == shipment.ShipmentTypeId).Description,
        shipment.ShipmentStatusId,
        BaseStaticEntity.GetAll<ShipmentStatus>().First(status => status.Id == shipment.ShipmentStatusId).Description,
        shipment.CarrierName, shipment.TrackingCode, shipment.EstimatedDelivery, shipment.ShippedAt, shipment.DeliveredAt);
}

public sealed record OrderItemDetailResponse(
    long Id, long? ProductId, long? ProductVariantId, long? ServiceId, string ItemName, string VariantName, string SKU,
    int Quantity, decimal UnitPrice, decimal DiscountAmount, decimal TotalPrice);

/// <summary>
/// Cobrança do pedido. CheckoutUrl e Instructions vêm do gateway (link de pagamento, código Pix, linha digitável).
/// </summary>
public sealed record PaymentInfoResponse(
    string Provider, long MethodId, string Method, long StatusId, string Status, decimal Amount, string Reference,
    string? CheckoutUrl, string? Instructions, DateTime? ExpiresAt, DateTime? PaidAt, DateTime? RefundedAt);

public sealed record OrderHistoryResponse(
    long? FromStatusId, string? FromStatus, long ToStatusId, string ToStatus, long? FromPaymentStatusId, long ToPaymentStatusId,
    string ToPaymentStatus, OrderChangeSource Source, long? ActorId, string Reason, DateTime ChangedAt)
{
    public static OrderHistoryResponse FromEntity(OrderStatusHistory history, bool includeActor) => new(
        history.FromStatusId, history.FromStatusId is long from ? OrderLabels.Status(from) : null,
        history.ToStatusId, OrderLabels.Status(history.ToStatusId),
        history.FromPaymentStatusId, history.ToPaymentStatusId, OrderLabels.PaymentStatus(history.ToPaymentStatusId),
        history.Source, includeActor ? history.ActorId : null, history.Reason, history.ChangedAt);
}

public sealed record OrderDetailResponse(
    long Id,
    long Number,
    long CustomerId,
    long StatusId,
    string Status,
    long PaymentStatusId,
    string PaymentStatus,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal ShippingCost,
    decimal TaxAmount,
    decimal TotalAmount,
    long? CouponId,
    string ShippingAddressLine,
    string ShippingCity,
    long ShippingStateId,
    string ShippingZipCode,
    string Notes,
    DateTime? PaymentExpiresAt,
    bool CanCustomerCancel,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDetailResponse> Items,
    PaymentInfoResponse? Payment,
    IReadOnlyList<ShipmentInfoResponse> Shipments,
    IReadOnlyList<OrderHistoryResponse> History)
{
    public static OrderDetailResponse FromEntity(OrderEntity order, bool backoffice)
    {
        var payment = order.ListTransaction
            .Where(transaction => transaction.TransactionTypeId == TransactionType.Payment.Id)
            .OrderByDescending(transaction => transaction.Id)
            .FirstOrDefault();

        return new(
            order.Id, order.OrderNumber, order.CustomerId, order.OrderStatusId, OrderLabels.Status(order.OrderStatusId),
            order.OrderPaymentStatusId, OrderLabels.PaymentStatus(order.OrderPaymentStatusId),
            order.SubTotal, order.DiscountAmount, order.ShippingCost, order.TaxAmount, order.TotalAmount, order.CouponId,
            order.ShippingAddressLine, order.ShippingCity, order.ShippingStateId, order.ShippingZipCode, order.Notes,
            order.PaymentExpiresAt, order.IsAwaitingPayment, order.CreatedAt,
            order.ListOrderItem.OrderBy(item => item.Id).Select(item => new OrderItemDetailResponse(
                item.Id, item.ProductId, item.ProductVariantId, item.ServiceId, item.ItemName, item.VariantName, item.SKU,
                item.Quantity, item.UnitPrice, item.DiscountAmount, item.TotalPrice)).ToList(),
            payment is null ? null : PaymentFromEntity(payment),
            order.ListShipment.OrderBy(shipment => shipment.Id).Select(ShipmentInfoResponse.FromEntity).ToList(),
            order.ListStatusHistory.OrderBy(history => history.ChangedAt).ThenBy(history => history.Id)
                .Select(history => OrderHistoryResponse.FromEntity(history, backoffice)).ToList());
    }

    private static PaymentInfoResponse PaymentFromEntity(Transaction payment)
    {
        var session = PaymentSessionPayload.Parse(payment.GatewayPayload);

        return new(payment.GatewayProvider, payment.TransactionMethodId,
            BaseStaticEntity.GetAll<TransactionMethod>().First(method => method.Id == payment.TransactionMethodId).Description,
            payment.TransactionStatusId,
            BaseStaticEntity.GetAll<TransactionStatus>().First(status => status.Id == payment.TransactionStatusId).Description,
            payment.Amount, payment.GatewayReference, session?.CheckoutUrl, session?.Instructions, session?.ExpiresAt,
            payment.PaidAt, payment.RefundedAt);
    }
}

public sealed record OrderSummaryResponse(
    long Id, long Number, long StatusId, string Status, long PaymentStatusId, string PaymentStatus, decimal TotalAmount,
    DateTime? PaymentExpiresAt, bool CanCustomerCancel, DateTime CreatedAt)
{
    public static OrderSummaryResponse FromEntity(OrderEntity order) => new(
        order.Id, order.OrderNumber, order.OrderStatusId, OrderLabels.Status(order.OrderStatusId), order.OrderPaymentStatusId,
        OrderLabels.PaymentStatus(order.OrderPaymentStatusId), order.TotalAmount, order.PaymentExpiresAt, order.IsAwaitingPayment, order.CreatedAt);
}

/// <summary>
/// Linha da busca de pedidos do backoffice.
/// </summary>
public sealed record OrderSearchItemResponse(
    long Id, long Number, long CustomerId, string CustomerName, long StatusId, string Status, long PaymentStatusId, string PaymentStatus,
    decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, long? CouponId, long? PaymentMethodId, DateTime CreatedAt)
{
    public static OrderSearchItemResponse FromEntity(OrderEntity order, string customerName) => new(
        order.Id, order.OrderNumber, order.CustomerId, customerName, order.OrderStatusId, OrderLabels.Status(order.OrderStatusId),
        order.OrderPaymentStatusId, OrderLabels.PaymentStatus(order.OrderPaymentStatusId), order.SubTotal, order.DiscountAmount,
        order.TotalAmount, order.CouponId, order.PaymentMethodId, order.CreatedAt);
}

/// <summary>
/// Dados da cobrança guardados em Transaction.GatewayPayload para reexibir as instruções ao cliente.
/// </summary>
public sealed record PaymentSessionPayload(string? CheckoutUrl, string? Instructions, DateTime? ExpiresAt)
{
    public string Serialize() => JsonSerializer.Serialize(this);

    public static PaymentSessionPayload? Parse(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        try
        {
            return JsonSerializer.Deserialize<PaymentSessionPayload>(payload);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

internal static class OrderLabels
{
    public static string Status(long id) => BaseStaticEntity.GetAll<OrderStatus>().First(status => status.Id == id).Description;

    public static string PaymentStatus(long id) => BaseStaticEntity.GetAll<OrderPaymentStatus>().First(status => status.Id == id).Description;
}
