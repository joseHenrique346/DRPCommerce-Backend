using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateListProductCommandHandler(
    CatalogWriteContext context,
    ProductDefaultVariantWriter defaults)
    : CatalogRequestHandler<UpdateListProductCommand, List<Product>>
{
    protected override async Task<List<Product>> ExecuteAsync(UpdateListProductCommand request, CancellationToken ct)
    {
        var commands = request.commands;

        ProductBatchIdentityPolicy.EnsureDistinct(commands.Select(Command => Command.id).ToList());
        await using var transaction = await context.BeginAsync(ct);
        var products = (await context.Products.GetWithVariantsAsync(commands.Select(Command => Command.id).ToList(), ct))
            .ToDictionary(Product => Product.Id);

        if (products.Count != commands.Count)
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado na loja atual.");

        foreach (var command in commands)
        {
            var product = products[command.id];

            CatalogWriteContext.EnsureEditable(product);

            if (product.CategoryId != command.categoryId || product.SupplierId != command.supplierId)
                await context.ValidateCategoryAsync(command.categoryId, command.supplierId, ct);

            if (product.IsDigital != command.isDigital)
                CatalogWriteContext.EnsureEmptyStock(product.ListProductVariant);

            product.UpdateDetails(command.name, command.slug, command.description, command.sku, command.barCode, command.brand, command.imageUrls, command.isDigital);
            product.UpdatePricing(command.price, command.costPrice);
            product.UpdateDimensions(command.weight, command.height, command.width, command.length);
            product.UpdateCategory(command.categoryId, command.supplierId);
            product.SetActive(command.isActive);
            defaults.Synchronize(product);
            await context.ValidateIdentityAsync(product, product.Id, ct);
        }

        ProductBatchIdentityPolicy.ValidateBatchIdentity(products.Values.ToList());
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return commands.Select(Command => products[Command.id]).ToList();
    }
}
