namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteListProductCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<DeleteListProductCommand, bool>
{
    protected override Task<bool> ExecuteAsync(DeleteListProductCommand request, CancellationToken ct)
    {
        return writer.DeleteAsync(request.ids, ct);
    }
}