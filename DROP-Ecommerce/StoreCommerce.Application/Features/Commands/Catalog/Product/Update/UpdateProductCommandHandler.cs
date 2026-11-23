using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductCommandHandler(
    CatalogWriteContext context,
    ProductDefaultVariantWriter defaults)
    : CatalogRequestHandler<UpdateProductCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(UpdateProductCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = (await context.Products.GetWithVariantsAsync([request.id], ct)).SingleOrDefault();

        if (product is null)
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado na loja atual.");

        CatalogWriteContext.EnsureEditable(product);

        if (product.CategoryId != request.categoryId || product.SupplierId != request.supplierId)
            await context.ValidateCategoryAsync(request.categoryId, request.supplierId, ct);

        if (product.IsDigital != request.isDigital)
            CatalogWriteContext.EnsureEmptyStock(product.ListProductVariant);

        product.UpdateDetails(request.name, request.slug, request.description, request.sku, request.barCode, request.brand, request.imageUrls, request.isDigital);
        product.UpdatePricing(request.price, request.costPrice);
        product.UpdateDimensions(request.weight, request.height, request.width, request.length);
        product.UpdateCategory(request.categoryId, request.supplierId);
        product.SetActive(request.isActive);
        defaults.Synchronize(product);
        await context.ValidateIdentityAsync(product, product.Id, ct);

        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
