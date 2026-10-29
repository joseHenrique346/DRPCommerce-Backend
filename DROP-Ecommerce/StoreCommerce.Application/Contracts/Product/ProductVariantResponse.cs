using System.Text.Json;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductVariantResponse(
    long Id, long ProductId, string Name, string SKU, string BarCode, Dictionary<string, string> Attributes,
    bool IsDefault, bool IsActive, decimal? PriceOverride, decimal? CostPriceOverride, decimal Price, decimal CostPrice,
    decimal Weight, decimal Height, decimal Width, decimal Length, int StockQuantity, int ReservedQuantity, int AvailableQuantity, long Version, bool TracksStock,
    decimal? WeightOverride, decimal? HeightOverride, decimal? WidthOverride, decimal? LengthOverride)
{
    public static ProductVariantResponse FromEntity(ProductVariant variant)
    {
        return new(variant.Id, variant.ProductId, variant.Name, variant.SKU, variant.BarCode,
            JsonSerializer.Deserialize<Dictionary<string, string>>(variant.AttributesJson)!, variant.IsDefault, variant.IsActive,
            variant.Price, variant.CostPrice, variant.Price ?? variant.Product.Price, variant.CostPrice ?? variant.Product.CostPrice,
            variant.Weight ?? variant.Product.Weight, variant.Height ?? variant.Product.Height,
            variant.Width ?? variant.Product.Width, variant.Length ?? variant.Product.Length,
            variant.StockQuantity, variant.ReservedQuantity, variant.AvailableQuantity, variant.Version,
            !variant.Product.IsDigital && (!variant.Product.HasVariants || !variant.IsDefault),
            variant.Weight, variant.Height, variant.Width, variant.Length);
    }
}
