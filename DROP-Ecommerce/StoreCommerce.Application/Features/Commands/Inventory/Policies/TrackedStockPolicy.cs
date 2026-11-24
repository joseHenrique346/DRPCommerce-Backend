using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public static class TrackedStockPolicy
{
    public static void EnsureTracked(ProductVariant variant)
    {
        if (variant.Product.IsDigital || (variant.Product.HasVariants && variant.IsDefault))
            throw new SecurityFailure(400, "Inventory.UntrackedSku", "O SKU padrão de um produto com variantes e os produtos digitais não controlam estoque.");
    }
}
