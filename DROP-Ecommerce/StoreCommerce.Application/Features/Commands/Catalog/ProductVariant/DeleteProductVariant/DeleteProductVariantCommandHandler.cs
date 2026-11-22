using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteProductVariantCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<DeleteProductVariantCommand, bool>
{
    protected override Task<bool> ExecuteAsync(DeleteProductVariantCommand request, CancellationToken ct)
    {
        return writer.DeleteAsync(request.ProductId, request.Id, ct);
    }
}
