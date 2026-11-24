using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductPricingCommandHandler(
    CatalogWriteContext context,
    ProductDefaultVariantWriter defaults)
    : CatalogRequestHandler<UpdateProductPricingCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(UpdateProductPricingCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(request.Id, ct);
        product.UpdatePricing(request.Price, request.CostPrice);
        defaults.Synchronize(product);
        await context.ValidateIdentityAsync(product, product.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
