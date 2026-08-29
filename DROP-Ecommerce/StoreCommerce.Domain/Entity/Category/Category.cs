using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Category : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long? ParentCategoryId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public Category? ParentCategory { get; private set; }
    private readonly List<Category> _listCategory = [];
    public IReadOnlyCollection<Category> ListCategory => _listCategory.AsReadOnly();
    private readonly List<Product> _listProduct = [];
    public IReadOnlyCollection<Product> ListProduct => _listProduct.AsReadOnly();
    private readonly List<Service> _listService = [];
    public IReadOnlyCollection<Service> ListService => _listService.AsReadOnly();
    #endregion

    #endregion

    #region Constructors
    protected Category() { }

    private Category(long enterpriseId, long? parentCategoryId, string name, string slug, string description, string imageUrl, int displayOrder, bool isActive)
    {
        EnterpriseId = enterpriseId;
        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    public static Category Create(long enterpriseId, long? parentCategoryId, string name, string slug, string description, string imageUrl, int displayOrder, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNullablePositive(parentCategoryId, nameof(ParentCategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 200, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, 100, nameof(Slug));
        BaseValidate.ValidateMaxLength(description, 500, nameof(Description));
        BaseValidate.ValidateMaxLength(imageUrl, 500, nameof(ImageUrl));
        BaseValidate.ValidatePositiveOrZero(displayOrder, nameof(DisplayOrder));

        return new Category(enterpriseId, parentCategoryId, name, slug, description, imageUrl, displayOrder, isActive);
    }

    public void UpdateDetails(long? parentCategoryId, string name, string slug, string description, string imageUrl, int displayOrder, bool isActive)
    {
        BaseValidate.ValidateNullablePositive(parentCategoryId, nameof(ParentCategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 200, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, 100, nameof(Slug));
        BaseValidate.ValidateMaxLength(description, 500, nameof(Description));
        BaseValidate.ValidateMaxLength(imageUrl, 500, nameof(ImageUrl));
        BaseValidate.ValidatePositiveOrZero(displayOrder, nameof(DisplayOrder));

        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
