using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CatalogWriteContext(
    IProductRepository products, ICategoryRepository categories, IRepository<ProductVariant> variants,
    IRepository<Supplier> suppliers, IUnitOfWork unitOfWork, ITenantProvider tenant)
{
    public IProductRepository Products { get; } = products;
    public IRepository<ProductVariant> Variants { get; } = variants;
    public IUnitOfWork UnitOfWork { get; } = unitOfWork;
    public long Tenant => tenant.GetEnterpriseId();

    public async Task<IStoreTransaction> BeginAsync(CancellationToken ct)
    {
        var transaction = await UnitOfWork.BeginTransactionAsync(ct);

        try
        {
            await Products.LockCatalogAsync(Tenant, ct);

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();

            throw;
        }
    }

    public async Task<Product> GetProductAsync(long id, CancellationToken ct)
    {
        return (await Products.GetWithVariantsAsync([id], ct)).SingleOrDefault()
            ?? throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado na loja atual.");
    }

    public async Task ValidateCategoryAsync(long categoryId, long? supplierId, CancellationToken ct)
    {
        var current = await categories.ListAsync(Category => Category.EnterpriseId == Tenant, ct, tracking: false);

        if (!CategoryRules.VisibleCategories(current).Any(Category => Category.Id == categoryId))
            throw new SecurityFailure(400, "Product.InvalidCategory", "A categoria e seus ancestrais devem estar ativos na loja atual.");

        if (supplierId.HasValue && !await suppliers.ExistsAsync(Supplier => Supplier.Id == supplierId && Supplier.EnterpriseId == Tenant, ct))
            throw new SecurityFailure(400, "Product.InvalidSupplier", "Fornecedor não encontrado na loja atual.");
    }

    public async Task ValidateIdentityAsync(Product product, long? existingId, CancellationToken ct)
    {
        if (await Products.ExistsAsync(Product => Product.EnterpriseId == Tenant && Product.Id != existingId && Product.Slug == product.Slug, ct))
            throw new SecurityFailure(409, "Product.DuplicateSlug", "Já existe um produto com este slug na loja.");

        if (await Variants.ExistsAsync(Variant => Variant.EnterpriseId == Tenant && Variant.SKU == product.SKU &&
            (!existingId.HasValue || Variant.ProductId != existingId || !Variant.IsDefault), ct))
            throw new SecurityFailure(409, "Product.DuplicateSku", "Este SKU já pertence a um produto ou variante da loja.");
    }

    public async Task ValidateVariantAsync(ProductVariant variant, long? existingId, CancellationToken ct)
    {
        if (await Variants.ExistsAsync(Variant => Variant.EnterpriseId == Tenant && Variant.Id != existingId && Variant.SKU == variant.SKU, ct))
            throw new SecurityFailure(409, "ProductVariant.DuplicateSku", "Este SKU já está em uso na loja.");

        if (await Variants.ExistsAsync(Variant => Variant.ProductId == variant.ProductId && Variant.Id != existingId &&
            Variant.CombinationKey == variant.CombinationKey, ct))
            throw new SecurityFailure(409, "ProductVariant.DuplicateCombination", "Esta combinação de opções já existe no produto.");
    }

    public static void EnsureEmptyStock(IEnumerable<ProductVariant> variants)
    {
        if (variants.Any(Variant => Variant.StockQuantity != 0 || Variant.ReservedQuantity != 0))
            throw new SecurityFailure(409, "Inventory.StockInUse", "Zere o saldo e libere as reservas dos SKUs antes desta operação.");
    }
}
