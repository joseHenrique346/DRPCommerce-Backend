namespace StoreCommerce.Application.Contracts.Store;

public sealed record StorefrontProductResponse(
    long Id, string Name, string Slug, string Description,
    decimal Price, string Brand, string ImageUrls, long CategoryId,
    bool HasVariants, bool IsDigital, decimal MaxPrice, long AvailableQuantity, bool InStock, bool CanPurchase)
{
    public static StorefrontProductResponse FromEntity(Domain.Entity.Product product, bool allowBackorder)
    {
        var variants = product.ListProductVariant.Where(Variant => !Variant.IsDeleted && Variant.IsActive &&
            (!product.HasVariants || !Variant.IsDefault)).ToList();
        var prices = variants.Select(Variant => Variant.Price ?? product.Price).DefaultIfEmpty(product.Price).ToList();
        var available = variants.Sum(Variant => (long)Variant.AvailableQuantity);
        var inStock = product.IsDigital || available > 0;

        return new(product.Id, product.Name, product.Slug, product.Description, prices.Min(), product.Brand, product.ImageUrls,
            product.CategoryId, product.HasVariants, product.IsDigital, prices.Max(), available, inStock,
            variants.Count > 0 && (inStock || allowBackorder));
    }
}
