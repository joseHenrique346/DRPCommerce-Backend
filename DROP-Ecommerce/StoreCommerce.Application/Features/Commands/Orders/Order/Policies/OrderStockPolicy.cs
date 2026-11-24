using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public static class OrderStockPolicy
{
    /// <summary>
    /// Produtos digitais e o SKU padrão de produtos com variantes não controlam estoque.
    /// </summary>
    public static bool ControlsStock(ProductVariant variant) =>
        !variant.Product.IsDigital && !(variant.Product.HasVariants && variant.IsDefault);
}
