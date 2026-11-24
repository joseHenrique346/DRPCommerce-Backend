using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetProductActiveCommandHandler(
    CatalogWriteContext context,
    ProductDefaultVariantWriter defaults)
    : CatalogRequestHandler<SetProductActiveCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(SetProductActiveCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(request.Id, ct);
        product.SetActive(request.IsActive);
        defaults.Synchronize(product);
        await context.ValidateIdentityAsync(product, product.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
