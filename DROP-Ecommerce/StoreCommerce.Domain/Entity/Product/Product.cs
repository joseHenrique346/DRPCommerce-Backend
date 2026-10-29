using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace StoreCommerce.Domain.Entity;

public class Product : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Constants
    public const int NameMaxLength = 255;
    public const int SlugMaxLength = 255;
    public const int DescriptionMaxLength = 500;
    public const int SkuMaxLength = 100;
    public const int BarCodeMaxLength = 100;
    public const int BrandMaxLength = 200;
    public const int ImageUrlsMaxLength = 500;
    public const decimal NumericMaxValue = 9999999999999999.99m;
    #endregion

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
    public bool HasVariants { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public Category Category { get; private set; }
    public Supplier? Supplier { get; private set; }
    private readonly List<OrderItem> _listOrderItem = [];
    public IReadOnlyCollection<OrderItem> ListOrderItem => _listOrderItem.AsReadOnly();
    private readonly List<ProductVariant> _listProductVariant = [];
    public IReadOnlyCollection<ProductVariant> ListProductVariant => _listProductVariant.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Product()
    {
    }

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
    public static Product Create(long enterpriseId, long categoryId, long? supplierId, string name, string slug, string description, string sku, string barCode, decimal price, decimal costPrice, decimal weight, decimal height, decimal width, decimal length, string brand, string imageUrls, bool isActive, bool isDigital, bool hasVariants = false)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(categoryId, nameof(CategoryId));
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));
        ValidateDetails(name, slug, description, sku, barCode, brand, imageUrls);
        ValidatePricing(price, costPrice);
        ValidateDimensions(isDigital, weight, height, width, length);

        var product = new Product(enterpriseId, categoryId, supplierId, name, slug, description, sku.Trim().ToUpperInvariant(), barCode,
            price, costPrice, weight, height, width, length, brand, imageUrls, isActive, isDigital);
        product.HasVariants = hasVariants;
        product._listProductVariant.Add(ProductVariant.Create(product, "Padrão", product.SKU, barCode,
            new Dictionary<string, string>(), isDefault: true, isActive: !hasVariants));

        return product;
    }

    public void UpdateDetails(string name, string slug, string description, string sku, string barCode, string brand, string imageUrls, bool isDigital)
    {
        ValidateDetails(name, slug, description, sku, barCode, brand, imageUrls);

        Name = name;
        Slug = slug;
        Description = description;
        SKU = sku.Trim().ToUpperInvariant();
        BarCode = barCode;
        Brand = brand;
        ImageUrls = imageUrls;
        IsDigital = isDigital;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void EnableVariants()
    {
        if (HasVariants)
            return;

        _listProductVariant.Single(Variant => Variant.IsDefault && !Variant.IsDeleted).DisableDefault();
        HasVariants = true;
    }

    public void SynchronizeDefaultVariant()
    {
        var variant = _listProductVariant.Single(Variant => Variant.IsDefault && !Variant.IsDeleted);
        variant.UpdateDetails("Padrão", SKU, BarCode, new Dictionary<string, string>(), !HasVariants);
    }

    public void UpdatePricing(decimal price, decimal costPrice)
    {
        ValidatePricing(price, costPrice);

        Price = price;
        CostPrice = costPrice;
    }

    public void UpdateDimensions(decimal weight, decimal height, decimal width, decimal length)
    {
        ValidateDimensions(IsDigital, weight, height, width, length);

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

    private static void ValidateDetails(string name, string slug, string description, string sku, string barCode, string brand, string imageUrls)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, NameMaxLength, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(slug, nameof(Slug));
        BaseValidate.ValidateMaxLength(slug, SlugMaxLength, nameof(Slug));

        if (!Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Slug de produto inválido.");

        BaseValidate.ValidateMaxLength(description, DescriptionMaxLength, nameof(Description));
        BaseValidate.ValidateNotNullOrEmpty(sku, nameof(SKU));
        BaseValidate.ValidateMaxLength(sku, SkuMaxLength, nameof(SKU));
        BaseValidate.ValidateMaxLength(barCode, BarCodeMaxLength, nameof(BarCode));
        BaseValidate.ValidateMaxLength(brand, BrandMaxLength, nameof(Brand));
        BaseValidate.ValidateMaxLength(imageUrls, ImageUrlsMaxLength, nameof(ImageUrls));
    }

    public static void ValidatePricing(decimal price, decimal costPrice)
    {
        BaseValidate.ValidateRange(price, 0.01m, NumericMaxValue, nameof(Price));
        BaseValidate.ValidateRange(costPrice, 0, NumericMaxValue, nameof(CostPrice));

        if (decimal.Round(price, 2) != price || decimal.Round(costPrice, 2) != costPrice)
            throw new ArgumentException("Preço e custo permitem no máximo duas casas decimais.");

        if (costPrice > price)
            throw new ArgumentException("O custo não pode ser maior que o preço de venda.");
    }

    public static void ValidateDimensions(bool isDigital, decimal weight, decimal height, decimal width, decimal length)
    {
        BaseValidate.ValidateRange(weight, 0, NumericMaxValue, nameof(Weight));
        BaseValidate.ValidateRange(height, 0, NumericMaxValue, nameof(Height));
        BaseValidate.ValidateRange(width, 0, NumericMaxValue, nameof(Width));
        BaseValidate.ValidateRange(length, 0, NumericMaxValue, nameof(Length));

        if (decimal.Round(weight, 2) != weight || decimal.Round(height, 2) != height ||
            decimal.Round(width, 2) != width || decimal.Round(length, 2) != length)
            throw new ArgumentException("Peso e dimensões permitem no máximo duas casas decimais.");

        if (!isDigital && (weight <= 0 || height <= 0 || width <= 0 || length <= 0))
            throw new ArgumentException("Produtos físicos exigem peso, altura, largura e comprimento positivos.");
    }
    #endregion
}
