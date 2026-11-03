using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StoreCommerce.Domain.Entity;

public class Category : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Constants
    public const int NameMaxLength = 200;
    public const int SlugMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int ImageUrlMaxLength = 500;
    public const int MaximumDepth = 3;
    public const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";
    #endregion

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
    protected Category()
    {
    }

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
    public static Category Create(long enterpriseId, long? parentCategoryId, string name, string? slug, string description, string imageUrl, int displayOrder, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        var category = new Category(enterpriseId, null, string.Empty, string.Empty, string.Empty, string.Empty, 0, isActive);
        category.UpdateDetails(parentCategoryId, name, slug, description, imageUrl, displayOrder, isActive);

        return category;
    }

    public void UpdateDetails(long? parentCategoryId, string name, string? slug, string description, string imageUrl, int displayOrder, bool isActive)
    {
        BaseValidate.ValidateNullablePositive(parentCategoryId, nameof(ParentCategoryId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, NameMaxLength, nameof(Name));
        slug = ResolveSlug(name, slug);
        BaseValidate.ValidateMaxLength(slug, SlugMaxLength, nameof(Slug));

        if (!Regex.IsMatch(slug, SlugPattern, RegexOptions.CultureInvariant))
            throw new ArgumentException("O slug deve conter apenas letras minúsculas, números e hífens.", nameof(slug));

        if (Id > 0 && parentCategoryId == Id)
            throw new ArgumentException("A categoria não pode ser sua própria categoria pai.", nameof(parentCategoryId));

        BaseValidate.ValidateMaxLength(description, DescriptionMaxLength, nameof(Description));
        BaseValidate.ValidateMaxLength(imageUrl, ImageUrlMaxLength, nameof(ImageUrl));
        BaseValidate.ValidatePositiveOrZero(displayOrder, nameof(DisplayOrder));

        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void SetDisplayOrder(int displayOrder)
    {
        BaseValidate.ValidatePositiveOrZero(displayOrder, nameof(DisplayOrder));

        DisplayOrder = displayOrder;
    }

    private static string ResolveSlug(string name, string? slug)
    {
        if (!string.IsNullOrWhiteSpace(slug))
            return slug;

        var normalized = name.Normalize(NormalizationForm.FormD);
        var characters = normalized.Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark);
        var value = Regex.Replace(new string(characters.ToArray()).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

        if (value.Length > SlugMaxLength)
            value = value[..SlugMaxLength].TrimEnd('-');

        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Slug));

        return value;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
