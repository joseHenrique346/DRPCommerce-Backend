using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Entity;

public class Coupon : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Constants
    public const int CodeMinLength = 3;
    public const int CodeMaxLength = 50;
    public const decimal MaxPercentage = 100m;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public string Code { get; private set; }
    public long CouponTypeId { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal MinOrderValue { get; private set; }
    public decimal MaxDiscountCap { get; private set; }
    public int? MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSingleUse { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public CouponType CouponType { get; private set; }
    private readonly List<Order> _listOrder = [];
    public IReadOnlyCollection<Order> ListOrder => _listOrder.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Coupon() { }

    private Coupon(long enterpriseId, string code, long couponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
    {
        EnterpriseId = enterpriseId;
        Code = code;
        CouponTypeId = couponTypeId;
        DiscountValue = discountValue;
        MinOrderValue = minOrderValue;
        MaxDiscountCap = maxDiscountCap;
        MaxUses = maxUses;
        UsedCount = 0;
        IsActive = isActive;
        IsSingleUse = isSingleUse;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
    }

    #endregion

    #region Functions
    public static Coupon Create(long enterpriseId, string code, long couponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        ValidateDetails(code, couponTypeId, discountValue, minOrderValue, maxDiscountCap, maxUses, startsAt, expiresAt);

        return new Coupon(enterpriseId, code, couponTypeId, discountValue, minOrderValue, maxDiscountCap, maxUses, isActive, isSingleUse, startsAt, expiresAt);
    }

    public void UpdateDetails(string code, long couponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
    {
        ValidateDetails(code, couponTypeId, discountValue, minOrderValue, maxDiscountCap, maxUses, startsAt, expiresAt);

        if (maxUses.HasValue && maxUses.Value < UsedCount)
            throw new ArgumentException($"{nameof(MaxUses)} cannot be lower than {nameof(UsedCount)}.");

        Code = code;
        CouponTypeId = couponTypeId;
        DiscountValue = discountValue;
        MinOrderValue = minOrderValue;
        MaxDiscountCap = maxDiscountCap;
        MaxUses = maxUses;
        IsSingleUse = isSingleUse;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
    }

    public void SetActive(bool active) => IsActive = active;

    public bool IsPercentage => CouponTypeId == CouponType.Percentage.Id;

    /// <summary>
    /// Indica se o cupom pode ser aplicado no momento informado (ativo, vigente e com usos disponíveis).
    /// </summary>
    public bool IsUsable(DateTime now) =>
        IsActive && !IsDeleted && StartsAt <= now && ExpiresAt > now && (!MaxUses.HasValue || UsedCount < MaxUses.Value);

    /// <summary>
    /// Calcula o desconto sobre o subtotal. Retorna zero quando o subtotal não atinge o valor mínimo do pedido.
    /// MaxDiscountCap igual a zero significa desconto percentual sem teto.
    /// </summary>
    public decimal CalculateDiscount(decimal subTotal)
    {
        if (subTotal <= 0 || subTotal < MinOrderValue)
            return 0;

        var discount = IsPercentage
            ? Math.Round(subTotal * DiscountValue / 100m, 2, MidpointRounding.AwayFromZero)
            : DiscountValue;

        if (IsPercentage && MaxDiscountCap > 0)
            discount = Math.Min(discount, MaxDiscountCap);

        return Math.Min(discount, subTotal);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private static void ValidateDetails(string code, long couponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, DateTime startsAt, DateTime expiresAt)
    {
        BaseValidate.ValidateNotNullOrEmpty(code, nameof(Code));
        BaseValidate.ValidateLength(code, CodeMinLength, CodeMaxLength, nameof(Code));
        BaseValidate.ValidatePositive(couponTypeId, nameof(CouponTypeId));

        if (!BaseStaticEntity.GetAll<CouponType>().Any(type => type.Id == couponTypeId))
            throw new ArgumentException($"{nameof(CouponTypeId)} is invalid.");

        BaseValidate.ValidatePositive(discountValue, nameof(DiscountValue));

        if (couponTypeId == CouponType.Percentage.Id && discountValue > MaxPercentage)
            throw new ArgumentException($"{nameof(DiscountValue)} cannot be greater than {MaxPercentage}% for percentage coupons.");

        BaseValidate.ValidatePositiveOrZero(minOrderValue, nameof(MinOrderValue));
        BaseValidate.ValidatePositiveOrZero(maxDiscountCap, nameof(MaxDiscountCap));
        BaseValidate.ValidateNullablePositive(maxUses, nameof(MaxUses));
        BaseValidate.ValidateGreaterThan(expiresAt, startsAt, nameof(ExpiresAt));
    }
    #endregion
}
