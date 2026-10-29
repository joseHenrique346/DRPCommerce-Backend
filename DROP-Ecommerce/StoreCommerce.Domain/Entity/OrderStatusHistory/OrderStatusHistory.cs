using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Quem originou a mudança de status do pedido.
/// </summary>
public enum OrderChangeSource
{
    Customer = 1,
    Backoffice = 2,
    System = 3,
    PaymentGateway = 4
}

/// <summary>
/// Auditoria das mudanças de status e de status de pagamento do pedido.
/// </summary>
public class OrderStatusHistory : BaseEntity, ITenantEntity
{
    #region Constants
    public const int ReasonMaxLength = 500;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public long OrderId { get; private set; }
    public long? FromStatusId { get; private set; }
    public long ToStatusId { get; private set; }
    public long? FromPaymentStatusId { get; private set; }
    public long ToPaymentStatusId { get; private set; }
    public OrderChangeSource Source { get; private set; }
    public long? ActorId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }

    #region Navigation Properties
    public Order Order { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructor
    protected OrderStatusHistory() { }
    #endregion

    #region Functions
    internal static OrderStatusHistory Create(Order order, long? fromStatusId, long? fromPaymentStatusId, OrderChangeSource source, long? actorId, string? reason, DateTime now)
    {
        var text = reason?.Trim() ?? string.Empty;
        BaseValidate.ValidateMaxLength(text, ReasonMaxLength, nameof(Reason));

        return new OrderStatusHistory
        {
            EnterpriseId = order.EnterpriseId,
            Order = order,
            OrderId = order.Id,
            FromStatusId = fromStatusId,
            ToStatusId = order.OrderStatusId,
            FromPaymentStatusId = fromPaymentStatusId,
            ToPaymentStatusId = order.OrderPaymentStatusId,
            Source = source,
            ActorId = actorId,
            Reason = text,
            ChangedAt = now
        };
    }
    #endregion
}
