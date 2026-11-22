using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductVariantCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<UpdateProductVariantCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.ProductId, request.Id, Variant => Variant.UpdateDetails(request.Request.Name,
            request.Request.SKU, request.Request.BarCode, request.Request.Attributes, request.Request.IsActive), ct);
    }
}
