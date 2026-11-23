using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public static class ProductBatchIdentityPolicy
{
    public static void ValidateBatchIdentity(List<Product> products)
    {
        if (products.GroupBy(Product => Product.Slug).Any(Group => Group.Count() > 1) ||
            products.GroupBy(Product => Product.SKU).Any(Group => Group.Count() > 1))
            throw new SecurityFailure(409, "Product.DuplicateIdentity", "O lote contém slugs ou SKUs duplicados.");
    }

    public static void EnsureDistinct(List<long> ids)
    {
        if (ids.Count != ids.Distinct().Count())
            throw new SecurityFailure(400, "Product.DuplicateIds", "O lote contém ids duplicados.");
    }
}
