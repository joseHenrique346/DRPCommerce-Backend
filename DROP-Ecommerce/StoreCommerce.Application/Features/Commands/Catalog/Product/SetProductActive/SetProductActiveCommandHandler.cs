using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetProductActiveCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<SetProductActiveCommand, Product>
{
    protected override Task<Product> ExecuteAsync(SetProductActiveCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.SetActive(request.IsActive), ct);
    }
}
