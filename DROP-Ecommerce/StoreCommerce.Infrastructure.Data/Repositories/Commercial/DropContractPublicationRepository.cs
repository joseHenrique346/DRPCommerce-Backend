using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class DropContractPublicationRepository(StoreCommerceDbContext db,
    IEntityRepository<DropContractPublication> publications) : IDropContractPublicationRepository
{
    public IEntityRepository<DropContractPublication> Publications => publications;

    public Task<List<Enterprise>> PendingEnterprisesAsync(CancellationToken ct)
    {
        return db.Set<DropContractPublication>().IgnoreQueryFilters().AsNoTracking()
            .Where(Publication => Publication.AcknowledgedRevision < Publication.Revision && Publication.Enterprise.IsActive)
            .OrderBy(Publication => Publication.ChangedAt).ThenBy(Publication => Publication.Id)
            .Select(Publication => Publication.Enterprise).Take(50).ToListAsync(ct);
    }

    public Task<DropContractSyncResponse?> StateAsync(long tenant, CancellationToken ct)
    {
        return db.Set<DropContractPublication>().IgnoreQueryFilters().AsNoTracking()
            .Where(Publication => Publication.EnterpriseId == tenant)
            .Select(Publication => new DropContractSyncResponse(Publication.Revision, Publication.AcknowledgedRevision,
                Publication.AcknowledgedRevision < Publication.Revision, Publication.ChangedAt, Publication.AcknowledgedAt))
            .SingleOrDefaultAsync(ct);
    }
}
