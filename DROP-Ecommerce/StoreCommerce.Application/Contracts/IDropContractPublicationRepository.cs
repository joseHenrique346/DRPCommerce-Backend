using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Contracts;

public interface IDropContractPublicationRepository
{
    IEntityRepository<DropContractPublication> Publications { get; }
    Task<List<Enterprise>> PendingEnterprisesAsync(CancellationToken ct);
    Task<DropContractSyncResponse?> StateAsync(long tenant, CancellationToken ct);
}
