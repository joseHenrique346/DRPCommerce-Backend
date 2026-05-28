using DropCommerce.Domain.StaticEntity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Domain.Entity;

public class DropCoupon : BaseEntity, ISoftDeletable
{
    #region Properties

    public long DropEventId { get; private set; }
    public string Code { get; private set; }
    public long DropCouponTypeId { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal MinOrderValue { get; private set; }
    public decimal MaxDiscountCap { get; private set; }
    public int MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSingleUse { get; private set; }
    public bool IsExclusiveToRegistered { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public DropCouponType DropCouponType { get; private set; }
    public ICollection<DropOrder> ListDropOrder { get; private set; } = [];

    #endregion

    #endregion

    #region Constructors

    protected DropCoupon() { }

    private DropCoupon(long dropEventId, string code, long dropCouponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int maxUses, int usedCount, bool isActive, bool isSingleUse, bool isExclusiveToRegistered, DateTime startsAt, DateTime expiresAt)
    {
        DropEventId = dropEventId;
        Code = code;
        DropCouponTypeId = dropCouponTypeId;
        DiscountValue = discountValue;
        MinOrderValue = minOrderValue;
        MaxDiscountCap = maxDiscountCap;
        MaxUses = maxUses;
        UsedCount = usedCount;
        IsActive = isActive;
        IsSingleUse = isSingleUse;
        IsExclusiveToRegistered = isExclusiveToRegistered;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
    }

    #endregion

    #region Functions

    public static DropCoupon Create(long dropEventId, string code, long dropCouponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int maxUses, int usedCount, bool isActive, bool isSingleUse, bool isExclusiveToRegistered, DateTime startsAt, DateTime expiresAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateString(code, nameof(code));
        BaseValidate.ValidateId(dropCouponTypeId, nameof(dropCouponTypeId));
        BaseValidate.ValidateMinimumDecimal(discountValue, 0.01m, nameof(discountValue));
        BaseValidate.ValidatePositiveDecimal(minOrderValue, nameof(minOrderValue));
        BaseValidate.ValidatePositiveDecimal(maxDiscountCap, nameof(maxDiscountCap));
        BaseValidate.ValidateMinimum(maxUses, 1, nameof(maxUses));
        BaseValidate.ValidatePositive(usedCount, nameof(usedCount));
        BaseValidate.ValidateDate(startsAt, nameof(startsAt));
        BaseValidate.ValidateDate(expiresAt, nameof(expiresAt));
        BaseValidate.ValidateDateRange(startsAt, expiresAt, nameof(startsAt), nameof(expiresAt));

        if (usedCount > maxUses)
            throw new ArgumentException("usedCount não pode exceder maxUses.");

        return new DropCoupon(dropEventId, code, dropCouponTypeId, discountValue, minOrderValue, maxDiscountCap, maxUses, usedCount, isActive, isSingleUse, isExclusiveToRegistered, startsAt, expiresAt);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    #endregion
}
