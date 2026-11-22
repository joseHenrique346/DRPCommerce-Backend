using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductPricingCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateProductPricingCommand, Product>
{
    protected override Task<Product> ExecuteAsync(UpdateProductPricingCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.UpdatePricing(request.Price, request.CostPrice), ct);
    }
}
