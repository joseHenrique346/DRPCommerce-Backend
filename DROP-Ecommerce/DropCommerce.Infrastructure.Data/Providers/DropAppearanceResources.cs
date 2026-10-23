using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.StaticEntity;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Providers;

public sealed class DropAppearanceResources(
    DropCommerceDbContext db,
    IStoreProductReader products,
    StoreIntegrationScope integration) : IAppearanceResources
{
    public async Task EnsureTargetAsync(long enterpriseId, InputAppearanceTarget target, bool publicAccess, CancellationToken ct)
    {
        if (enterpriseId < 1 ||
            target.AppearanceTargetTypeId != AppearanceTargetType.DropDefault.Id && target.AppearanceTargetTypeId != AppearanceTargetType.DropSpecific.Id ||
            target.AppearanceTargetTypeId == AppearanceTargetType.DropDefault.Id && target.DropEventId.HasValue ||
            target.AppearanceTargetTypeId == AppearanceTargetType.DropSpecific.Id && target.DropEventId is not > 0)
            throw NotFound();

        if (target.DropEventId.HasValue)
        {
            var available = await Events(enterpriseId).AnyAsync(DropEvent =>
                DropEvent.Id == target.DropEventId && (!publicAccess || DropEvent.IsPublic), ct);

            if (!available)
                throw NotFound();
        }
    }

    public async Task<bool> ExistsAsync(long enterpriseId, InputAppearanceTarget target, string kind, long id, CancellationToken ct)
    {
        using var operation = integration.Begin(enterpriseId);

        if (kind != "product" || await products.GetByIdAsync(id, enterpriseId, ct) is not { IsActive: true })
            return false;

        if (!target.DropEventId.HasValue)
            return true;

        return await Events(enterpriseId).AnyAsync(DropEvent => DropEvent.Id == target.DropEventId && DropEvent.ProductId == id, ct) ||
            await db.DropProducts.IgnoreQueryFilters().AnyAsync(DropProduct =>
                DropProduct.DropEventId == target.DropEventId && DropProduct.ProductId == id && !DropProduct.IsDeleted &&
                DropProduct.DropEvent.EnterpriseId == enterpriseId && !DropProduct.DropEvent.IsDeleted, ct);
    }

    private IQueryable<DropEvent> Events(long enterpriseId)
    {
        return db.DropEvents.IgnoreQueryFilters().Where(DropEvent => DropEvent.EnterpriseId == enterpriseId && !DropEvent.IsDeleted);
    }

    private static AppearanceFailure NotFound()
    {
        return new AppearanceFailure(404, "Appearance.TargetNotFound", "Evento indisponível.");
    }
}
