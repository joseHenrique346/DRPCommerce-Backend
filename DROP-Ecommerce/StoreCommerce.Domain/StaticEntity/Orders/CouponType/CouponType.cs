namespace StoreCommerce.Domain.StaticEntity;

public sealed class CouponType : BaseStaticEntity
{
    public static readonly CouponType Percentage = new(1, "Percentual");
    public static readonly CouponType FixedAmount = new(2, "Valor fixo");

    private CouponType(long id, string description) : base(id, description) { }
}
