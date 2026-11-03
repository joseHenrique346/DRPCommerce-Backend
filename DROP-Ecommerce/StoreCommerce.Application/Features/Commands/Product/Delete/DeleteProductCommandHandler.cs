namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteProductCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<DeleteProductCommand, bool>
{
    protected override Task<bool> ExecuteAsync(DeleteProductCommand request, CancellationToken ct)
    {
        return writer.DeleteAsync([request.id], ct);
    }
}
