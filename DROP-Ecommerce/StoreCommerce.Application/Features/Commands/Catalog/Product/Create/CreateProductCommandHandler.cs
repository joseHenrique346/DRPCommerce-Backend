using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateProductCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<CreateProductCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(CreateProductCommand request, CancellationToken ct)
    {
        return (await writer.CreateAsync([request], ct))[0];
    }
}
