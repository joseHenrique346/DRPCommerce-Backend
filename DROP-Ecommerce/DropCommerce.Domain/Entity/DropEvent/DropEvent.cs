namespace DropCommerce.Domain.Entity;

public class DropEvent : BaseEntity
{
    #region Properties

    public long EnterpriseId { get; private set; }
    public long ProductId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public string CoverImageUrl { get; private set; }
    public string BannerImageUrl { get; private set; }
    public long StatusId { get; private set; }
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

    #endregion

    #region Constructors

    protected DropEvent() { }

    private DropEvent(long enterpriseId, long productId, string name, string slug, string description, string coverImageUrl, string bannerImageUrl, long statusId, int totalUnitsAvailable, int unitsReserved, int unitsSold, decimal price, bool requiresRegistration, bool isPublic, DateTime registrationStartsAt, DateTime registrationEndsAt, DateTime queueOpensAt, DateTime dropStartsAt, DateTime dropEndsAt)
    {
        EnterpriseId = enterpriseId;
        ProductId = productId;
        Name = name;
        Slug = slug;
        Description = description;
        CoverImageUrl = coverImageUrl;
        BannerImageUrl = bannerImageUrl;
        StatusId = statusId;
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

    public static DropEvent Create(long enterpriseId, long productId, string name, string slug, string description, string coverImageUrl, string bannerImageUrl, long statusId, int totalUnitsAvailable, int unitsReserved, int unitsSold, decimal price, bool requiresRegistration, bool isPublic, DateTime registrationStartsAt, DateTime registrationEndsAt, DateTime queueOpensAt, DateTime dropStartsAt, DateTime dropEndsAt)
    {
        BaseValidate<long>.ValidateNotNullValue(enterpriseId);
        BaseValidate<long>.ValidateIdValue(enterpriseId);

        BaseValidate<long>.ValidateNotNullValue(productId);
        BaseValidate<long>.ValidateIdValue(productId);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(name);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(slug);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(description);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(coverImageUrl);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(bannerImageUrl);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<int>.ValidateNotNullValue(totalUnitsAvailable);
        BaseValidate<int>.ValidateNotNullValue(unitsReserved);
        BaseValidate<int>.ValidateNotNullValue(unitsSold);

        BaseValidate<decimal>.ValidateNotNullValue(price);

        BaseValidate<DateTime>.ValidateNotNullValue(registrationStartsAt);
        BaseValidate<DateTime>.ValidateNotNullValue(registrationEndsAt);
        BaseValidate<DateTime>.ValidateNotNullValue(queueOpensAt);
        BaseValidate<DateTime>.ValidateNotNullValue(dropStartsAt);
        BaseValidate<DateTime>.ValidateNotNullValue(dropEndsAt);

        return new DropEvent(enterpriseId, productId, name, slug, description, coverImageUrl, bannerImageUrl, statusId, totalUnitsAvailable, unitsReserved, unitsSold, price, requiresRegistration, isPublic, registrationStartsAt, registrationEndsAt, queueOpensAt, dropStartsAt, dropEndsAt);
    }

    #endregion
}