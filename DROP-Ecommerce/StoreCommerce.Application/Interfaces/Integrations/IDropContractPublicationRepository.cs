using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.Integrations;

public interface IDropContractPublicationRepository
{
    IEntityRepository<DropContractPublication> Publications { get; }
    Task<List<Enterprise>> PendingEnterprisesAsync(CancellationToken ct);
    Task<DropContractSyncResponse?> StateAsync(long tenant, CancellationToken ct);
}
