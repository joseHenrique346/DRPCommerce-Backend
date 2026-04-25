namespace DropCommerce.Domain.Entity;

public class DropEvent : BaseEntity
{
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

    public DropEvent() { }

    public DropEvent(long enterpriseId, long productId, string name, string slug, string description, string coverImageUrl, string bannerImageUrl, long statusId, int totalUnitsAvailable, int unitsReserved, int unitsSold, decimal price, bool requiresRegistration, bool isPublic, DateTime registrationStartsAt, DateTime registrationEndsAt, DateTime queueOpensAt, DateTime dropStartsAt, DateTime dropEndsAt)
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
}
