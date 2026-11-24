using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductDetailsCommandHandler(
    CatalogWriteContext context,
    ProductDefaultVariantWriter defaults)
    : CatalogRequestHandler<UpdateProductDetailsCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(UpdateProductDetailsCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(request.Id, ct);

        if (product.IsDigital != request.IsDigital)
            CatalogWriteContext.EnsureEmptyStock(product.ListProductVariant);

        product.UpdateDetails(request.Name, request.Slug, request.Description, request.SKU, request.BarCode,
            request.Brand, request.ImageUrls, request.IsDigital);
        defaults.Synchronize(product);
        await context.ValidateIdentityAsync(product, product.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
