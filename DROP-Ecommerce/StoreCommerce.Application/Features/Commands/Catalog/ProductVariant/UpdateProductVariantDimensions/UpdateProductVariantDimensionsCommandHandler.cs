using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductVariantDimensionsCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<UpdateProductVariantDimensionsCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantDimensionsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.ProductId, request.Id, Variant => Variant.UpdateDimensions(request.Request.Weight,
            request.Request.Height, request.Request.Width, request.Request.Length), ct);
    }
}
