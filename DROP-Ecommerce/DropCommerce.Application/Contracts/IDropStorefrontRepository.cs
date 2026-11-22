using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Contracts;

public interface IDropStorefrontRepository : IDropCommercePersistence
{
    Task<DropStorefrontEventResponse?> FindPublicEventAsync(long id, CancellationToken ct);
    Task<List<DropStorefrontEventSummary>> ListPublicEventsAsync(CancellationToken ct);
    Task LockEventAsync(long id, long tenant, CancellationToken ct);
    Task<DropEvent?> FindEventAsync(long id, CancellationToken ct);
    Task<bool> HasEligibleProductAsync(long id, CancellationToken ct);
    Task<DropRegistration?> FindRegistrationAsync(long id, long customerId, CancellationToken ct);
    Task<List<DropCustomerRegistrationResponse>> ListRegistrationsAsync(CancellationToken ct);
}
