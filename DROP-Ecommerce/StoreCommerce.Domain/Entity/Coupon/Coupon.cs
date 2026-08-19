namespace StoreCommerce.Domain.Entity.Base;

public class Coupon : BaseEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string Code { get; private set; }
    public long TypeId { get; private set; }
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
    #endregion

    #region Constructor
    protected Coupon() { }

    private Coupon(long enterpriseId, string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, int usedCount, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
    {
        EnterpriseId = enterpriseId;
        Code = code;
        TypeId = typeId;
        DiscountValue = discountValue;
        MinOrderValue = minOrderValue;
        MaxDiscountCap = maxDiscountCap;
        MaxUses = maxUses;
        UsedCount = usedCount;
        IsActive = isActive;
        IsSingleUse = isSingleUse;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
    }
    #endregion

    #region Functions
    public static Coupon Create(long enterpriseId, string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, int usedCount, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(code, nameof(Code));
        BaseValidate.ValidateLength(code, 3, 50, nameof(Code));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidatePositive(discountValue, nameof(DiscountValue));
        BaseValidate.ValidatePositiveOrZero(minOrderValue, nameof(MinOrderValue));
        BaseValidate.ValidatePositiveOrZero(maxDiscountCap, nameof(MaxDiscountCap));
        BaseValidate.ValidateNullablePositive(maxUses, nameof(MaxUses));
        BaseValidate.ValidatePositiveOrZero(usedCount, nameof(UsedCount));
        BaseValidate.ValidateGreaterThan(expiresAt, startsAt, nameof(ExpiresAt));

        return new Coupon(enterpriseId, code, typeId, discountValue, minOrderValue, maxDiscountCap, maxUses, usedCount, isActive, isSingleUse, startsAt, expiresAt);
    }

    public void UpdateDetails(string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
    {
        BaseValidate.ValidateNotNullOrEmpty(code, nameof(Code));
        BaseValidate.ValidateLength(code, 3, 50, nameof(Code));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidatePositive(discountValue, nameof(DiscountValue));
        BaseValidate.ValidatePositiveOrZero(minOrderValue, nameof(MinOrderValue));
        BaseValidate.ValidatePositiveOrZero(maxDiscountCap, nameof(MaxDiscountCap));
        BaseValidate.ValidateNullablePositive(maxUses, nameof(MaxUses));
        BaseValidate.ValidateGreaterThan(expiresAt, startsAt, nameof(ExpiresAt));

        Code = code;
        TypeId = typeId;
        DiscountValue = discountValue;
        MinOrderValue = minOrderValue;
        MaxDiscountCap = maxDiscountCap;
        MaxUses = maxUses;
        IsSingleUse = isSingleUse;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
    }
    #endregion
}
