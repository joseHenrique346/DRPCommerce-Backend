using StoreCommerce.Domain.Enums.Orders;
using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record OrderDetailResponse(long Id, long Number, long CustomerId, long StatusId, string Status, long PaymentStatusId, string PaymentStatus, decimal SubTotal, decimal DiscountAmount, decimal ShippingCost, decimal TaxAmount, decimal TotalAmount, long? CouponId, string ShippingAddressLine, string ShippingCity, long ShippingStateId, string ShippingZipCode, string Notes, DateTime? PaymentExpiresAt, bool CanCustomerCancel, DateTime CreatedAt, IReadOnlyList<OrderItemDetailResponse> Items, PaymentInfoResponse? Payment, IReadOnlyList<ShipmentInfoResponse> Shipments, IReadOnlyList<OrderHistoryResponse> History, BuyerSnapshotResponse? Buyer = null, IReadOnlyList<PaymentOperationInfoResponse>? PaymentOperations = null)
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
            payment is null ? null : PaymentFromEntity(payment, order.ListPaymentAttempt.FirstOrDefault(Attempt => Attempt.TransactionId == payment.Id)),
            order.ListShipment.OrderBy(shipment => shipment.Id).Select(ShipmentInfoResponse.FromEntity).ToList(),
            order.ListStatusHistory.OrderBy(history => history.ChangedAt).ThenBy(history => history.Id)
                .Select(history => OrderHistoryResponse.FromEntity(history, backoffice)).ToList(),
            new BuyerSnapshotResponse(order.BuyerName, order.BuyerEmail, order.BuyerPhone,
                order.BuyerSnapshotCapturedAt, order.BuyerSnapshotSource),
            order.ListPaymentOperation.OrderBy(Operation => Operation.Id)
                .Select(Operation => new PaymentOperationInfoResponse(Operation.Id, Operation.Kind, Operation.Status,
                    Operation.Amount, Operation.CurrencyCode, Operation.ResultReference, Operation.ResolutionCode, Operation.CompletedAt)).ToList());
    }

    private static PaymentInfoResponse PaymentFromEntity(Transaction payment, PaymentAttempt? attempt)
    {
        var session = PaymentSessionPayload.Parse(payment.GatewayPayload);

        return new(payment.GatewayProvider, payment.TransactionMethodId,
            BaseStaticEntity.GetAll<TransactionMethod>().First(method => method.Id == payment.TransactionMethodId).Description,
            payment.TransactionStatusId,
            BaseStaticEntity.GetAll<TransactionStatus>().First(status => status.Id == payment.TransactionStatusId).Description,
            payment.Amount, payment.GatewayReference, session?.CheckoutUrl, session?.Instructions, session?.ExpiresAt,
            payment.PaidAt, payment.RefundedAt,
            attempt is null ? null : new PaymentAttemptInfoResponse(attempt.Id, attempt.Status, attempt.CurrencyCode,
                attempt.PreferenceId, attempt.Payments.OrderBy(Payment => Payment.Id).Select(Payment => Payment.PaymentId).ToList()));
    }
}
