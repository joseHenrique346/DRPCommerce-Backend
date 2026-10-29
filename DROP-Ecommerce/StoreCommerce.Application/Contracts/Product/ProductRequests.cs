namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductDetailsRequest(string Name, string Slug, string Description, string SKU, string BarCode, string Brand, string ImageUrls, bool IsDigital);
public sealed record ProductPricingRequest(decimal Price, decimal CostPrice);
public sealed record ProductDimensionsRequest(decimal Weight, decimal Height, decimal Width, decimal Length);
public sealed record ProductCategoryRequest(long CategoryId, long? SupplierId);
public sealed record ProductActiveRequest(bool IsActive);
public sealed record VariantDetailsRequest(string Name, string SKU, string BarCode, Dictionary<string, string> Attributes, bool IsActive = true);
public sealed record VariantPricingRequest(decimal? Price, decimal? CostPrice);
public sealed record VariantDimensionsRequest(decimal? Weight, decimal? Height, decimal? Width, decimal? Length);
public sealed record CreateVariantRequest(VariantDetailsRequest Details, VariantPricingRequest? Pricing = null, VariantDimensionsRequest? Dimensions = null);
