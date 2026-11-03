using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateListProductCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<CreateListProductCommand, List<Product>>
{
    protected override Task<List<Product>> ExecuteAsync(CreateListProductCommand request, CancellationToken ct)
    {
        return writer.CreateAsync(request.commands, ct);
    }
}