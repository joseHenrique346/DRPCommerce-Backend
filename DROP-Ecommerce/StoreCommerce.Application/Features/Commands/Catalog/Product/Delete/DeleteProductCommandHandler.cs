using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteProductCommandHandler(
    CatalogWriteContext context)
    : CatalogRequestHandler<DeleteProductCommand, bool>
{
    protected override async Task<bool> ExecuteAsync(DeleteProductCommand request, CancellationToken ct)
    {
        List<long> ids = [request.id];

        ProductBatchIdentityPolicy.EnsureDistinct(ids);
        await using var transaction = await context.BeginAsync(ct);
        var products = await context.Products.GetWithVariantsAsync(ids, ct);

        if (products.Count != ids.Count)
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado na loja atual.");

        CatalogWriteContext.EnsureEmptyStock(products.Where(Product => !Product.IsContractBlocked)
            .SelectMany(Product => Product.ListProductVariant));

        if (products.SelectMany(Product => Product.ListProductVariant).Any(Variant => Variant.ReservedQuantity != 0))
            throw new SecurityFailure(409, "Inventory.StockInUse", "Libere as reservas ativas antes de excluir o produto.");

        foreach (var product in products)
        {
            foreach (var variant in product.ListProductVariant)
                variant.SoftDelete(preserveAvailableStock: product.IsContractBlocked);

            product.SoftDelete();
        }

        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
