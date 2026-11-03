using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Uso comprometido do cupom por um pedido. O checkout o registra antes do pagamento; rascunhos administrativos,
/// na confirmação. Cancelamento, expiração e recusa de pagamento liberam o compromisso por ReleasedAt.
/// </summary>
public class CouponRedemption : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long CouponId { get; private set; }
    public long CustomerId { get; private set; }
    public long OrderId { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTime RedeemedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }

    #region Navigation Properties
    public Coupon Coupon { get; private set; } = null!;
    public Customer Customer { get; private set; } = null!;
    public Order Order { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructor
    protected CouponRedemption() { }
    #endregion

    #region Functions
    public bool IsActive => ReleasedAt is null;

    public static CouponRedemption Create(Coupon coupon, Order order, DateTime now)
    {
        BaseValidate.ValidateNotNull(coupon, nameof(Coupon));
        BaseValidate.ValidateNotNull(order, nameof(Order));

        if (coupon.EnterpriseId != order.EnterpriseId || order.CouponId != coupon.Id)
            throw new ArgumentException("Cupom não pertence ao pedido.");

        return new CouponRedemption
        {
            EnterpriseId = order.EnterpriseId,
            CouponId = coupon.Id,
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            Order = order,
            DiscountAmount = order.DiscountAmount,
            RedeemedAt = now
        };
    }

    public void Release(DateTime now) => ReleasedAt ??= now;
    #endregion
}
