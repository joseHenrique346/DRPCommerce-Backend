using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Linha da busca de pedidos do backoffice.
/// </summary>
public sealed record OrderSearchItemResponse(long Id, long Number, long CustomerId, string CustomerName, long StatusId, string Status, long PaymentStatusId, string PaymentStatus, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, long? CouponId, long? PaymentMethodId, DateTime CreatedAt)
{
    public static OrderSearchItemResponse FromEntity(OrderEntity order, string customerName) => new(
        order.Id, order.OrderNumber, order.CustomerId, customerName, order.OrderStatusId, OrderLabels.Status(order.OrderStatusId),
        order.OrderPaymentStatusId, OrderLabels.PaymentStatus(order.OrderPaymentStatusId), order.SubTotal, order.DiscountAmount,
        order.TotalAmount, order.CouponId, order.PaymentMethodId, order.CreatedAt);
}
