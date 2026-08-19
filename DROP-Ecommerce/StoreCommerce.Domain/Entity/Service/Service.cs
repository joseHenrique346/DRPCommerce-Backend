using StoreCommerce.Domain.Entity.Base;

using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Service : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    #endregion

    #region Constructor
    protected Service() { }

    private Service(long enterpriseId, long categoryId, string name, string description, decimal price, int durationMinutes, bool isActive)
    {
        EnterpriseId = enterpriseId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    public static Service Create(long enterpriseId, long categoryId, string name, string description, decimal price, int durationMinutes, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(categoryId, nameof(CategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateMaxLength(description, 5000, nameof(Description));
        BaseValidate.ValidatePositive(price, nameof(Price));
        BaseValidate.ValidatePositive(durationMinutes, nameof(DurationMinutes));

        return new Service(enterpriseId, categoryId, name, description, price, durationMinutes, isActive);
    }

    public void UpdateDetails(long categoryId, string name, string description, decimal price, int durationMinutes)
    {
        BaseValidate.ValidatePositive(categoryId, nameof(CategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateMaxLength(description, 5000, nameof(Description));
        BaseValidate.ValidatePositive(price, nameof(Price));
        BaseValidate.ValidatePositive(durationMinutes, nameof(DurationMinutes));

        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
    }

    public void UpdatePricing(decimal price)
    {
        BaseValidate.ValidatePositive(price, nameof(Price));

        Price = price;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
