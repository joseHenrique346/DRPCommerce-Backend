using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductCatalogWriter(CatalogWriteContext context)
{
    public async Task<List<Product>> CreateAsync(List<CreateProductCommand> commands, CancellationToken ct)
    {
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

        ValidateBatchIdentity(products);
        await context.Products.AddRangeAsync(products, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return products;
    }

    public async Task<List<Product>> UpdateAsync(List<UpdateProductCommand> commands, CancellationToken ct)
    {
        EnsureDistinct(commands.Select(Command => Command.id).ToList());
        await using var transaction = await context.BeginAsync(ct);
        var products = (await context.Products.GetWithVariantsAsync(commands.Select(Command => Command.id).ToList(), ct))
            .ToDictionary(Product => Product.Id);

        if (products.Count != commands.Count)
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado na loja atual.");

        foreach (var command in commands)
        {
            var product = products[command.id];

            if (product.CategoryId != command.categoryId || product.SupplierId != command.supplierId)
                await context.ValidateCategoryAsync(command.categoryId, command.supplierId, ct);

            if (product.IsDigital != command.isDigital)
                CatalogWriteContext.EnsureEmptyStock(product.ListProductVariant);

            product.UpdateDetails(command.name, command.slug, command.description, command.sku, command.barCode, command.brand, command.imageUrls, command.isDigital);
            product.UpdatePricing(command.price, command.costPrice);
            product.UpdateDimensions(command.weight, command.height, command.width, command.length);
            product.UpdateCategory(command.categoryId, command.supplierId);
            product.SetActive(command.isActive);
            product.SynchronizeDefaultVariant();
            ValidateVariants(product);
            await context.ValidateIdentityAsync(product, product.Id, ct);
        }

        ValidateBatchIdentity(products.Values.ToList());
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return commands.Select(Command => products[Command.id]).ToList();
    }

    public async Task<Product> MutateAsync(long id, Action<Product> mutation, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(id, ct);
        mutation(product);
        product.SynchronizeDefaultVariant();
        ValidateVariants(product);
        await context.ValidateIdentityAsync(product, product.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }

    public async Task<Product> ChangeCategoryAsync(long id, long categoryId, long? supplierId, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(id, ct);
        await context.ValidateCategoryAsync(categoryId, supplierId, ct);
        product.UpdateCategory(categoryId, supplierId);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return product;
    }

    public async Task<bool> DeleteAsync(List<long> ids, CancellationToken ct)
    {
        EnsureDistinct(ids);
        await using var transaction = await context.BeginAsync(ct);
        var products = await context.Products.GetWithVariantsAsync(ids, ct);

        if (products.Count != ids.Count)
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado na loja atual.");

        CatalogWriteContext.EnsureEmptyStock(products.SelectMany(Product => Product.ListProductVariant));

        foreach (var product in products)
        {
            foreach (var variant in product.ListProductVariant)
                variant.SoftDelete();

            product.SoftDelete();
        }

        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }

    private static void ValidateVariants(Product product)
    {
        foreach (var variant in product.ListProductVariant.Where(Variant => !Variant.IsDeleted))
            variant.ValidateAgainstProduct();
    }

    private static void ValidateBatchIdentity(List<Product> products)
    {
        if (products.GroupBy(Product => Product.Slug).Any(Group => Group.Count() > 1) ||
            products.GroupBy(Product => Product.SKU).Any(Group => Group.Count() > 1))
            throw new SecurityFailure(409, "Product.DuplicateIdentity", "O lote contém slugs ou SKUs duplicados.");
    }

    private static void EnsureDistinct(List<long> ids)
    {
        if (ids.Count != ids.Distinct().Count())
            throw new SecurityFailure(400, "Product.DuplicateIds", "O lote contém ids duplicados.");
    }
}
