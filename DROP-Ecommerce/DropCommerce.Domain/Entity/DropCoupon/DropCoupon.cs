namespace DropCommerce.Domain.Entity;

public class DropCoupon : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public string Code { get; private set; }
    public long TypeId { get; private set; }
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

    #endregion

    #region Constructors

    protected DropCoupon() { }

    private DropCoupon(long dropEventId, string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int maxUses, int usedCount, bool isActive, bool isSingleUse, bool isExclusiveToRegistered, DateTime startsAt, DateTime expiresAt)
    {
        DropEventId = dropEventId;
        Code = code;
        TypeId = typeId;
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

    public static DropCoupon Create(long dropEventId, string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int maxUses, int usedCount, bool isActive, bool isSingleUse, bool isExclusiveToRegistered, DateTime startsAt, DateTime expiresAt)
    {
        return new DropCoupon(dropEventId, code, typeId, discountValue, minOrderValue, maxDiscountCap, maxUses, usedCount, isActive, isSingleUse, isExclusiveToRegistered, startsAt, expiresAt);
    }

    #endregion
}