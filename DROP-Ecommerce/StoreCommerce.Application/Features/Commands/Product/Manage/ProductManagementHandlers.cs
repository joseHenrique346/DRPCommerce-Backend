using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductDetailsCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateProductDetailsCommand, Product>
{
    protected override Task<Product> ExecuteAsync(UpdateProductDetailsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product =>
        {
            if (Product.IsDigital != request.IsDigital)
                CatalogWriteContext.EnsureEmptyStock(Product.ListProductVariant);

            Product.UpdateDetails(request.Name, request.Slug, request.Description, request.SKU, request.BarCode,
                request.Brand, request.ImageUrls, request.IsDigital);
        }, ct);
    }
}

public sealed class UpdateProductPricingCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateProductPricingCommand, Product>
{
    protected override Task<Product> ExecuteAsync(UpdateProductPricingCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.UpdatePricing(request.Price, request.CostPrice), ct);
    }
}

public sealed class UpdateProductDimensionsCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateProductDimensionsCommand, Product>
{
    protected override Task<Product> ExecuteAsync(UpdateProductDimensionsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.UpdateDimensions(request.Weight, request.Height, request.Width, request.Length), ct);
    }
}

public sealed class ChangeProductCategoryCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<ChangeProductCategoryCommand, Product>
{
    protected override Task<Product> ExecuteAsync(ChangeProductCategoryCommand request, CancellationToken ct)
    {
        return writer.ChangeCategoryAsync(request.Id, request.CategoryId, request.SupplierId, ct);
    }
}

public sealed class SetProductActiveCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<SetProductActiveCommand, Product>
{
    protected override Task<Product> ExecuteAsync(SetProductActiveCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.SetActive(request.IsActive), ct);
    }
}

public sealed class EnableProductVariantsCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<EnableProductVariantsCommand, Product>
{
    protected override Task<Product> ExecuteAsync(EnableProductVariantsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.EnableVariants(), ct);
    }
}
