using System.Text.Json;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Store;

public sealed record StorefrontVariantResponse(long Id, string Name, string SKU, string BarCode,
    Dictionary<string, string> Attributes, decimal Price, decimal Weight, decimal Height, decimal Width, decimal Length,
    int AvailableQuantity, bool InStock, bool CanPurchase)
{
    public static StorefrontVariantResponse FromEntity(ProductVariant variant, bool allowBackorder)
    {
        var product = variant.Product;
        var inStock = product.IsDigital || variant.AvailableQuantity > 0;

        return new(variant.Id, variant.Name, variant.SKU, variant.BarCode,
            JsonSerializer.Deserialize<Dictionary<string, string>>(variant.AttributesJson)!, variant.Price ?? product.Price,
            variant.Weight ?? product.Weight, variant.Height ?? product.Height, variant.Width ?? product.Width,
            variant.Length ?? product.Length, variant.AvailableQuantity, inStock, inStock || allowBackorder);
    }
}

public sealed record StorefrontProductDetailResponse(StorefrontProductResponse Product, List<StorefrontVariantResponse> Variants);
