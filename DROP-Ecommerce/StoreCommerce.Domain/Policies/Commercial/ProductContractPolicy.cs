using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Contracts;

public static class ProductContractPolicy
{
    public const string ProductLimit = "product_limit";
    public const string StoreModule = "store_module";

    public static IReadOnlyList<Product> SelectExcess(IEnumerable<Product> products, int limit)
    {
        if (limit < 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        return products.Where(Product => !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked)
            .OrderBy(Product => Product.CreatedAt)
            .ThenBy(Product => Product.Id)
            .Skip(limit)
            .ToList();
    }
}
