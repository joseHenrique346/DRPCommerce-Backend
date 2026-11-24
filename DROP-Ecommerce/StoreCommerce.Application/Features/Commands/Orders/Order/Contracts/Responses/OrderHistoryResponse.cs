using StoreCommerce.Domain.Enums.Orders;
using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record OrderHistoryResponse(long? FromStatusId, string? FromStatus, long ToStatusId, string ToStatus, long? FromPaymentStatusId, long ToPaymentStatusId, string ToPaymentStatus, OrderChangeSource Source, long? ActorId, string Reason, DateTime ChangedAt)
{
    public static OrderHistoryResponse FromEntity(OrderStatusHistory history, bool includeActor) => new(
        history.FromStatusId, history.FromStatusId is long from ? OrderLabels.Status(from) : null,
        history.ToStatusId, OrderLabels.Status(history.ToStatusId),
        history.FromPaymentStatusId, history.ToPaymentStatusId, OrderLabels.PaymentStatus(history.ToPaymentStatusId),
        history.Source, includeActor ? history.ActorId : null, history.Reason, history.ChangedAt);
}
