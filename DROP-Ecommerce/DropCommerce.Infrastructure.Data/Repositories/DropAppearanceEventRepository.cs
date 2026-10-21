using DropCommerce.Application.Appearance;
using DropCommerce.Domain.Entity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropAppearanceEventRepository(DropCommerceDbContext db) : IDropAppearanceEventRepository
{
    public Task<DropEvent?> GetAsync(long enterpriseId, long eventId, CancellationToken ct)
    {
        return db.DropEvents.IgnoreQueryFilters()
            .SingleOrDefaultAsync(DropEvent => DropEvent.EnterpriseId == enterpriseId && DropEvent.Id == eventId && !DropEvent.IsDeleted, ct);
    }

    public Task<long?> GetPublicIdBySlugAsync(long enterpriseId, string slug, CancellationToken ct)
    {
        return db.DropEvents.IgnoreQueryFilters()
            .Where(DropEvent => DropEvent.EnterpriseId == enterpriseId && DropEvent.Slug == slug &&
                DropEvent.IsPublic && !DropEvent.IsDeleted)
            .Select(DropEvent => (long?)DropEvent.Id)
            .SingleOrDefaultAsync(ct);
    }
}
