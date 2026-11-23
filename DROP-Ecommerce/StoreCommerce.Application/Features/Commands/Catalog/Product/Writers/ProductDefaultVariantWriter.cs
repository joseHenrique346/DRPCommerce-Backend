using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductDefaultVariantWriter
{
    public void Synchronize(Product product)
    {
        product.SynchronizeDefaultVariant();

        foreach (var variant in product.ListProductVariant.Where(Variant => !Variant.IsDeleted))
            variant.ValidateAgainstProduct();
    }
}
