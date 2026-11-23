using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ChangeProductCategoryCommandHandler(
    CatalogWriteContext context)
    : CatalogRequestHandler<ChangeProductCategoryCommand, Product>
{
    protected override async Task<Product> ExecuteAsync(ChangeProductCategoryCommand request, CancellationToken ct)
    {
        var id = request.Id;
        var categoryId = request.CategoryId;
        var supplierId = request.SupplierId;

        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(id, ct);
        await context.ValidateCategoryAsync(categoryId, supplierId, ct);
        product.UpdateCategory(categoryId, supplierId);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }
}
