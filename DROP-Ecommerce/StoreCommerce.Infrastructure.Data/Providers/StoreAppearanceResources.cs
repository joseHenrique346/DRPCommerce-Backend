using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Application.Contracts.Appearance;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Appearance;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed class StoreAppearanceResources(StoreCommerceDbContext db) : IAppearanceResources
{
    public async Task EnsureTargetAsync(long enterpriseId, InputAppearanceTarget target, bool publicAccess, CancellationToken ct)
    {
        if (target != StoreAppearanceTargets.Default || !await db.Enterprises.IgnoreQueryFilters()
            .AnyAsync(Enterprise => Enterprise.Id == enterpriseId && (!publicAccess || Enterprise.IsActive), ct))
            throw new AppearanceFailure(404, "Appearance.TargetNotFound", "Loja indisponível.");
    }

    public async Task<bool> ExistsAsync(long enterpriseId, InputAppearanceTarget target, string kind, long id, CancellationToken ct)
    {
        return kind switch
        {
            "product" => await db.Products.IgnoreQueryFilters().AnyAsync(Product =>
                Product.Id == id && Product.EnterpriseId == enterpriseId && Product.IsActive && !Product.IsDeleted, ct),
            "category" => await db.Categories.IgnoreQueryFilters().AnyAsync(Category =>
                Category.Id == id && Category.EnterpriseId == enterpriseId && Category.IsActive && !Category.IsDeleted, ct),
            _ => false
        };
    }
}
