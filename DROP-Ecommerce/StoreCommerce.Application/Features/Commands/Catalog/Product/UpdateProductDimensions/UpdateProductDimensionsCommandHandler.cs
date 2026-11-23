using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductDimensionsCommandHandler(
    CatalogWriteContext context,
    ProductDefaultVariantWriter defaults)
    : CatalogRequestHandler<UpdateProductDimensionsCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(UpdateProductDimensionsCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(request.Id, ct);
        product.UpdateDimensions(request.Weight, request.Height, request.Width, request.Length);
        defaults.Synchronize(product);
        await context.ValidateIdentityAsync(product, product.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
