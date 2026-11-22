using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Contracts.Coupon;

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
