using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateProductCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(UpdateProductCommand request, CancellationToken ct)
    {
        return (await writer.UpdateAsync([request], ct))[0];
    }
}
