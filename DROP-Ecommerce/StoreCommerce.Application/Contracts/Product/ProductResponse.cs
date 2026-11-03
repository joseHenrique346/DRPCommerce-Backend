using ProductEntity = StoreCommerce.Domain.Entity.Product;

namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductResponse(long Id, long EnterpriseId, long CategoryId, long? SupplierId, string Name,
    string Slug, string Description, string SKU, string BarCode, decimal Price, decimal CostPrice,
    decimal Weight, decimal Height, decimal Width, decimal Length, string Brand, string ImageUrls,
    bool IsActive, bool IsDigital, bool HasVariants, DateTime CreatedAt, DateTime? UpdatedAt, List<ProductVariantResponse> Variants)
{
    public static ProductResponse FromEntity(ProductEntity product)
    {
        return new(product.Id, product.EnterpriseId, product.CategoryId, product.SupplierId, product.Name, product.Slug,
            product.Description, product.SKU, product.BarCode, product.Price, product.CostPrice,
            product.Weight, product.Height, product.Width, product.Length, product.Brand, product.ImageUrls,
            product.IsActive, product.IsDigital, product.HasVariants, product.CreatedAt, product.UpdatedAt,
            product.ListProductVariant.Where(Variant => !Variant.IsDeleted).OrderBy(Variant => Variant.Id)
                .Select(ProductVariantResponse.FromEntity).ToList());
    }
}
