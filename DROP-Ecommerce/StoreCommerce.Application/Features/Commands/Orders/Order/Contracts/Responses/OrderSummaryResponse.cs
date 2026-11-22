using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record OrderSummaryResponse(long Id, long Number, long StatusId, string Status, long PaymentStatusId, string PaymentStatus, decimal TotalAmount, DateTime? PaymentExpiresAt, bool CanCustomerCancel, DateTime CreatedAt)
{
    public static OrderSummaryResponse FromEntity(OrderEntity order) => new(
        order.Id, order.OrderNumber, order.OrderStatusId, OrderLabels.Status(order.OrderStatusId), order.OrderPaymentStatusId,
        OrderLabels.PaymentStatus(order.OrderPaymentStatusId), order.TotalAmount, order.PaymentExpiresAt, order.IsAwaitingPayment, order.CreatedAt);
}
