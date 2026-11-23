using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateListProductCommandHandler(
    CatalogWriteContext context)
    : CatalogRequestHandler<CreateListProductCommand, List<Product>>
{
    protected override async Task<List<Product>> ExecuteAsync(CreateListProductCommand request, CancellationToken ct)
    {
        var commands = request.commands;

        await using var transaction = await context.BeginAsync(ct);
        var products = new List<Product>();

        foreach (var command in commands)
        {
            await context.ValidateCategoryAsync(command.categoryId, command.supplierId, ct);
            var product = Product.Create(context.Tenant, command.categoryId, command.supplierId, command.name, command.slug,
                command.description, command.sku, command.barCode, command.price, command.costPrice, command.weight,
                command.height, command.width, command.length, command.brand, command.imageUrls, command.isActive, command.isDigital, command.hasVariants);
            await context.ValidateIdentityAsync(product, null, ct);
            products.Add(product);
        }

        ProductBatchIdentityPolicy.ValidateBatchIdentity(products);
        await context.Products.AddRangeAsync(products, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return products;
    }
}
