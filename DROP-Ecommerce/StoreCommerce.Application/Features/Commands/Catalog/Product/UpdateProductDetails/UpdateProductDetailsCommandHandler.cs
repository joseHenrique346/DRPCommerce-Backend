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
