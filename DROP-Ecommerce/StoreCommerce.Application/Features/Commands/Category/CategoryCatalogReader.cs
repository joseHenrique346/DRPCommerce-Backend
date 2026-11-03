using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CategoryCatalogReader(ICategoryRepository repository, ITenantProvider tenantProvider, StoreSettingsReader settings)
{
    public async Task<Result<List<Category>>> ReadAsync(bool storefront, CancellationToken cancellationToken)
    {
        if (storefront && !await settings.IsStoreActiveAsync(cancellationToken))
            return Result<List<Category>>.NotFound("Store.Unavailable", "Loja indisponível.");

        var tenant = tenantProvider.GetEnterpriseId();
        var categories = await repository.ListAsync(Category => Category.EnterpriseId == tenant, cancellationToken, tracking: false);

        return Result<List<Category>>.Success(storefront ? CategoryRules.VisibleCategories(categories) : categories);
    }
}
