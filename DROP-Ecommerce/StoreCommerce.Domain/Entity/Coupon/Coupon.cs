using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity.Coupon
{
    public class Coupon : BaseEntity, ISoftDeletable, ITenantEntity
    {
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

        public Coupon() { }

        public Coupon(long enterpriseId, string code, long couponTypeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, int usedCount, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt)
        {
            EnterpriseId = enterpriseId;
            Code = code;
            CouponTypeId = couponTypeId;
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

        public void SoftDelete() { IsDeleted = true; DeletedAt = DateTime.UtcNow; }
    }
}
