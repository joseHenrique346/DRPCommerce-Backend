using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateListProductCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<UpdateListProductCommand, List<Product>>
{
    protected override Task<List<Product>> ExecuteAsync(UpdateListProductCommand request, CancellationToken ct)
    {
        return writer.UpdateAsync(request.commands, ct);
    }
}