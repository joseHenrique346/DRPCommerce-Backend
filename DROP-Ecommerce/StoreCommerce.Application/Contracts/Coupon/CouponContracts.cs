using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Contracts.Coupon;

/// <summary>
/// Condição do cupom. Exemplos: valor do pedido (1) maior que (2) 1000; compras anteriores do cliente (3) igual a (1) 0.
/// SecondValue é usado apenas pelos operadores de intervalo (6 = entre, 7 = maior que e menor que).
/// </summary>
public sealed record CouponConditionRequest(long TypeId, long OperatorId, decimal Value, decimal? SecondValue);

public sealed record CouponConditionResponse(long TypeId, string Type, long OperatorId, string Operator, decimal Value, decimal? SecondValue, string Description)
{
    public static CouponConditionResponse FromEntity(CouponCondition condition) => new(
        condition.CouponConditionTypeId,
        BaseStaticEntity.GetAll<CouponConditionType>().First(type => type.Id == condition.CouponConditionTypeId).Description,
        condition.CouponConditionOperatorId,
        BaseStaticEntity.GetAll<CouponConditionOperator>().First(item => item.Id == condition.CouponConditionOperatorId).Description,
        condition.Value,
        condition.SecondValue,
        condition.Describe());
}

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

/// <summary>
/// Resultado da validação de um código. Quando inválido, ReasonCode e Message explicam o motivo.
/// </summary>
public sealed record CouponValidationResponse(string Code, bool IsValid, decimal DiscountAmount, decimal SubTotalAfterDiscount, string? ReasonCode, string? Message, IReadOnlyList<CouponConditionResponse> Conditions);
