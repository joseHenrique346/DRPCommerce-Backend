using StoreCommerce.Domain.Entity.Base;

using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity.Category;

public class Category : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long? ParentCategoryId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Dscription { get; private set; }
    public string ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    #endregion

    #region Constructor
    protected Category() { }

    private Category(long enterpriseId, long? parentCategoryId, string name, string slug, string dscription, string imageUrl, int displayOrder, bool isActive)
    {
        EnterpriseId = enterpriseId;
        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Dscription = dscription;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    public static Category Create(long enterpriseId, long? parentCategoryId, string name, string slug, string dscription, string imageUrl, int displayOrder, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNullablePositive(parentCategoryId, nameof(ParentCategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, 255, nameof(Slug));
        BaseValidate.ValidateMaxLength(dscription, 2000, nameof(Dscription));
        BaseValidate.ValidateMaxLength(imageUrl, 1000, nameof(ImageUrl));
        BaseValidate.ValidateUrlFormat(imageUrl, nameof(ImageUrl));
        BaseValidate.ValidatePositiveOrZero(displayOrder, nameof(DisplayOrder));

        return new Category(enterpriseId, parentCategoryId, name, slug, dscription, imageUrl, displayOrder, isActive);
    }

    public void UpdateDetails(long? parentCategoryId, string name, string slug, string dscription, string imageUrl, int displayOrder)
    {
        BaseValidate.ValidateNullablePositive(parentCategoryId, nameof(ParentCategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, 255, nameof(Slug));
        BaseValidate.ValidateMaxLength(dscription, 2000, nameof(Dscription));
        BaseValidate.ValidateMaxLength(imageUrl, 1000, nameof(ImageUrl));
        BaseValidate.ValidateUrlFormat(imageUrl, nameof(ImageUrl));
        BaseValidate.ValidatePositiveOrZero(displayOrder, nameof(DisplayOrder));

        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Dscription = dscription;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
