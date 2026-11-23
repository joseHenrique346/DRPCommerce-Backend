using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateProductCommandHandler(
    CatalogWriteContext context)
    : CatalogRequestHandler<CreateProductCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(CreateProductCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);

        await context.ValidateCategoryAsync(request.categoryId, request.supplierId, ct);
        var product = Product.Create(context.Tenant, request.categoryId, request.supplierId, request.name, request.slug,
            request.description, request.sku, request.barCode, request.price, request.costPrice, request.weight,
            request.height, request.width, request.length, request.brand, request.imageUrls, request.isActive, request.isDigital, request.hasVariants);
        await context.ValidateIdentityAsync(product, null, ct);

        await context.Products.AddRangeAsync([product], ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
