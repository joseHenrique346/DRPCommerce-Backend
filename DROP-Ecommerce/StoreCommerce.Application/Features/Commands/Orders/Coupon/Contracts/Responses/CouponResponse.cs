using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Contracts.Coupon;

/// <summary>
/// UsedCount inclui os usos comprometidos em pedidos pendentes de pagamento e pagos, descontados os liberados.
/// </summary>
public sealed record CouponResponse(long Id, string Code, long TypeId, string Type, decimal DiscountValue, decimal MaxDiscountCap, int? MaxUses, int UsedCount, bool IsActive, bool IsSingleUse, DateTime StartsAt, DateTime ExpiresAt, CouponAvailability Availability, IReadOnlyList<CouponConditionResponse> Conditions, DateTime CreatedAt, DateTime? UpdatedAt)
{
    public static CouponResponse FromEntity(Domain.Entity.Coupon coupon, DateTime now) => new(
        coupon.Id,
        coupon.Code,
        coupon.CouponTypeId,
        BaseStaticEntity.GetAll<CouponType>().First(type => type.Id == coupon.CouponTypeId).Description,
        coupon.DiscountValue,
        coupon.MaxDiscountCap,
        coupon.MaxUses,
        coupon.UsedCount,
        coupon.IsActive,
        coupon.IsSingleUse,
        coupon.StartsAt,
        coupon.ExpiresAt,
        coupon.GetAvailability(now),
        coupon.ListCondition.Select(CouponConditionResponse.FromEntity).ToList(),
        coupon.CreatedAt,
        coupon.UpdatedAt);
}
