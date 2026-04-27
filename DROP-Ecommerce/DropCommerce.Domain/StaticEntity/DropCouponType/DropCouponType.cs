namespace DropCommerce.Domain.StaticEntity;

public sealed class DropCouponType : BaseStaticEntity
{
    public static readonly DropCouponType Percentage = new(1, "Percentual");
    public static readonly DropCouponType FixedAmount = new(2, "Valor fixo");

    private DropCouponType(long id, string description) : base(id, description) { }
}