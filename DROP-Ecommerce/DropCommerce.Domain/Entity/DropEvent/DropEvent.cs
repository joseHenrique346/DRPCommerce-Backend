using DropCommerce.Domain.Interfaces;
using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class DropEvent : BaseEntity, ISoftDeletable
{
    #region Properties

    public long EnterpriseId { get; private set; }
    public long ProductId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public string CoverImageUrl { get; private set; }
    public string BannerImageUrl { get; private set; }
    public long DropEventStatusId { get; private set; }
    public int TotalUnitsAvailable { get; private set; }
    public int UnitsReserved { get; private set; }
    public int UnitsSold { get; private set; }
    public decimal Price { get; private set; }
    public bool RequiresRegistration { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime RegistrationStartsAt { get; private set; }
    public DateTime RegistrationEndsAt { get; private set; }
    public DateTime QueueOpensAt { get; private set; }
    public DateTime DropStartsAt { get; private set; }
    public DateTime DropEndsAt { get; private set; }

    #region Navigation Properties

    public DropEventStatus DropEventStatus { get; private set; }
    public ICollection<DropProduct> ListDropProduct { get; private set; } = [];
    public ICollection<DropOrder> ListDropOrder { get; private set; } = [];
    public ICollection<DropCoupon> ListDropCoupon { get; private set; } = [];
    public ICollection<DropRegistration> ListDropRegistration { get; private set; } = [];
    public ICollection<DropReservation> ListDropReservation { get; private set; } = [];
    public ICollection<QueueEntry> ListQueueEntry { get; private set; } = [];
    public ICollection<WaitlistEntry> ListWaitlistEntry { get; private set; } = [];
    public ICollection<DropNotification> ListDropNotification { get; private set; } = [];
    public ICollection<DropAuditLog> ListDropAuditLog { get; private set; } = [];
    public ICollection<FraudSignal> ListFraudSignal { get; private set; } = [];

    #endregion

    #endregion

    #region Constructors

    protected DropEvent() { }

    private DropEvent(long enterpriseId, long productId, string name, string slug, string description, string coverImageUrl, string bannerImageUrl, long dropEventStatusId, int totalUnitsAvailable, int unitsReserved, int unitsSold, decimal price, bool requiresRegistration, bool isPublic, DateTime registrationStartsAt, DateTime registrationEndsAt, DateTime queueOpensAt, DateTime dropStartsAt, DateTime dropEndsAt)
    {
        EnterpriseId = enterpriseId;
        ProductId = productId;
        Name = name;
        Slug = slug;
        Description = description;
        CoverImageUrl = coverImageUrl;
        BannerImageUrl = bannerImageUrl;
        DropEventStatusId = dropEventStatusId;
        TotalUnitsAvailable = totalUnitsAvailable;
        UnitsReserved = unitsReserved;
        UnitsSold = unitsSold;
        Price = price;
        RequiresRegistration = requiresRegistration;
        IsPublic = isPublic;
        RegistrationStartsAt = registrationStartsAt;
        RegistrationEndsAt = registrationEndsAt;
        QueueOpensAt = queueOpensAt;
        DropStartsAt = dropStartsAt;
        DropEndsAt = dropEndsAt;
    }

    #endregion

    #region Functions

    public static DropEvent Create(long enterpriseId, long productId, string name, string slug, string description, string coverImageUrl, string bannerImageUrl, long dropEventStatusId, int totalUnitsAvailable, int unitsReserved, int unitsSold, decimal price, bool requiresRegistration, bool isPublic, DateTime registrationStartsAt, DateTime registrationEndsAt, DateTime queueOpensAt, DateTime dropStartsAt, DateTime dropEndsAt)
    {
        BaseValidate.ValidateId(enterpriseId, nameof(enterpriseId));
        BaseValidate.ValidateId(productId, nameof(productId));
        BaseValidate.ValidateString(name, nameof(name));
        BaseValidate.ValidateString(slug, nameof(slug));
        BaseValidate.ValidateString(description, nameof(description));
        BaseValidate.ValidateString(coverImageUrl, nameof(coverImageUrl));
        BaseValidate.ValidateString(bannerImageUrl, nameof(bannerImageUrl));
        BaseValidate.ValidateId(dropEventStatusId, nameof(dropEventStatusId));
        BaseValidate.ValidateMinimum(totalUnitsAvailable, 1, nameof(totalUnitsAvailable));
        BaseValidate.ValidatePositive(unitsReserved, nameof(unitsReserved));
        BaseValidate.ValidatePositive(unitsSold, nameof(unitsSold));
        BaseValidate.ValidateMinimumDecimal(price, 0.01m, nameof(price));
        BaseValidate.ValidateDate(registrationStartsAt, nameof(registrationStartsAt));
        BaseValidate.ValidateDate(registrationEndsAt, nameof(registrationEndsAt));
        BaseValidate.ValidateDateRange(registrationStartsAt, registrationEndsAt, nameof(registrationStartsAt), nameof(registrationEndsAt));
        BaseValidate.ValidateDate(queueOpensAt, nameof(queueOpensAt));
        BaseValidate.ValidateDate(dropStartsAt, nameof(dropStartsAt));
        BaseValidate.ValidateDate(dropEndsAt, nameof(dropEndsAt));
        BaseValidate.ValidateDateRange(dropStartsAt, dropEndsAt, nameof(dropStartsAt), nameof(dropEndsAt));

        return new DropEvent(enterpriseId, productId, name, slug, description, coverImageUrl, bannerImageUrl, dropEventStatusId, totalUnitsAvailable, unitsReserved, unitsSold, price, requiresRegistration, isPublic, registrationStartsAt, registrationEndsAt, queueOpensAt, dropStartsAt, dropEndsAt);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    #endregion
}
