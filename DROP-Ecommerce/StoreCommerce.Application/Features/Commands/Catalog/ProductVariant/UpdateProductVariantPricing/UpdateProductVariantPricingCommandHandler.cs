using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductVariantPricingCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<UpdateProductVariantPricingCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantPricingCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.ProductId, request.Id, Variant => Variant.UpdatePricing(request.Request.Price, request.Request.CostPrice), ct);
    }
}
