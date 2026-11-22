using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ChangeProductCategoryCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<ChangeProductCategoryCommand, Product>
{
    protected override Task<Product> ExecuteAsync(ChangeProductCategoryCommand request, CancellationToken ct)
    {
        return writer.ChangeCategoryAsync(request.Id, request.CategoryId, request.SupplierId, ct);
    }
}
