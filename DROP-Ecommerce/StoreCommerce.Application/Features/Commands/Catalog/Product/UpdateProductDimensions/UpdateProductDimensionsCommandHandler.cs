using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductDimensionsCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateProductDimensionsCommand, Product>
{
    protected override Task<Product> ExecuteAsync(UpdateProductDimensionsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.UpdateDimensions(request.Weight, request.Height, request.Width, request.Length), ct);
    }
}
