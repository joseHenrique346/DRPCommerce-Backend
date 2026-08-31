using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Product : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long CategoryId { get; private set; }
    public long? SupplierId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public string SKU { get; private set; }
    public string BarCode { get; private set; }
    public decimal Price { get; private set; }
    public decimal CostPrice { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Height { get; private set; }
    public decimal Width { get; private set; }
    public decimal Length { get; private set; }
    public string Brand { get; private set; }
    public string ImageUrls { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDigital { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    #endregion

    #region Constructor
    protected Product() { }

    private Product(long enterpriseId, long categoryId, long? supplierId, string name, string slug, string description, string sku, string barCode, decimal price, decimal costPrice, decimal weight, decimal height, decimal width, decimal length, string brand, string imageUrls, bool isActive, bool isDigital)
    {
        EnterpriseId = enterpriseId;
        CategoryId = categoryId;
        SupplierId = supplierId;
        Name = name;
        Slug = slug;
        Description = description;
        SKU = sku;
        BarCode = barCode;
        Price = price;
        CostPrice = costPrice;
        Weight = weight;
        Height = height;
        Width = width;
        Length = length;
        Brand = brand;
        ImageUrls = imageUrls;
        IsActive = isActive;
        IsDigital = isDigital;
    }
    #endregion

    #region Functions
    public static Product Create(long enterpriseId, long categoryId, long? supplierId, string name, string slug, string description, string sku, string barCode, decimal price, decimal costPrice, decimal weight, decimal height, decimal width, decimal length, string brand, string imageUrls, bool isActive, bool isDigital)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(categoryId, nameof(CategoryId));
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, 255, nameof(Slug));
        BaseValidate.ValidateMaxLength(description, 5000, nameof(Description));
        BaseValidate.ValidateMaxLength(sku, 100, nameof(SKU));
        BaseValidate.ValidateMaxLength(barCode, 100, nameof(BarCode));
        BaseValidate.ValidatePositive(price, nameof(Price));
        BaseValidate.ValidatePositiveOrZero(costPrice, nameof(CostPrice));
        BaseValidate.ValidatePositiveOrZero(weight, nameof(Weight));
        BaseValidate.ValidatePositiveOrZero(height, nameof(Height));
        BaseValidate.ValidatePositiveOrZero(width, nameof(Width));
        BaseValidate.ValidatePositiveOrZero(length, nameof(Length));
        BaseValidate.ValidateMaxLength(brand, 255, nameof(Brand));
        BaseValidate.ValidateMaxLength(imageUrls, 2000, nameof(ImageUrls));

        return new Product(enterpriseId, categoryId, supplierId, name, slug, description, sku, barCode, price, costPrice, weight, height, width, length, brand, imageUrls, isActive, isDigital);
    }

    public void UpdateDetails(string name, string slug, string description, string brand, string imageUrls)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, 255, nameof(Slug));
        BaseValidate.ValidateMaxLength(description, 5000, nameof(Description));
        BaseValidate.ValidateMaxLength(brand, 255, nameof(Brand));
        BaseValidate.ValidateMaxLength(imageUrls, 2000, nameof(ImageUrls));

        Name = name;
        Slug = slug;
        Description = description;
        Brand = brand;
        ImageUrls = imageUrls;
    }

    public void UpdatePricing(decimal price, decimal costPrice)
    {
        BaseValidate.ValidatePositive(price, nameof(Price));
        BaseValidate.ValidatePositiveOrZero(costPrice, nameof(CostPrice));

        Price = price;
        CostPrice = costPrice;
    }

    public void UpdateDimensions(decimal weight, decimal height, decimal width, decimal length)
    {
        BaseValidate.ValidatePositiveOrZero(weight, nameof(Weight));
        BaseValidate.ValidatePositiveOrZero(height, nameof(Height));
        BaseValidate.ValidatePositiveOrZero(width, nameof(Width));
        BaseValidate.ValidatePositiveOrZero(length, nameof(Length));

        Weight = weight;
        Height = height;
        Width = width;
        Length = length;
    }

    public void UpdateCategory(long categoryId, long? supplierId)
    {
        BaseValidate.ValidatePositive(categoryId, nameof(CategoryId));
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));

        CategoryId = categoryId;
        SupplierId = supplierId;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}