using DropCommerce.Application.Contracts;
using DropCommerce.Application.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Interfaces.Queue;

public interface IDropQueueRepository : IDropCommercePersistence
{
    Task<DropEvent?> FindEventAsync(long eventId, CancellationToken ct);
    Task<bool> IsRegisteredAsync(long eventId, long customer, CancellationToken ct);
    Task<QueueEntry?> FindEntryAsync(long eventId, long customer, CancellationToken ct);
    Task<QueueEntry?> FindCustomerEntryAsync(long eventId, long customer, long tenant, CancellationToken ct);
    Task<DropQueueSessionResponse?> FindActiveSessionAsync(long entryId, long customer, DateTime now, CancellationToken ct);
    Task<QueueSession?> FindSessionAsync(long sessionId, long customer, long eventId, long tenant, CancellationToken ct);
    Task<DropReservation?> FindReservationAsync(long sessionId, long customer, string key, CancellationToken ct);
    Task<DropProduct?> FindParticipationAsync(long participationId, long eventId, long tenant, CancellationToken ct);
    Task<long> ReservedUnitsAsync(long eventId, long productId, string identityHash, DateTime now, CancellationToken ct);
    Task<int> PurchaseLimitAsync(long eventId, long productId, long tenant, CancellationToken ct);
    Task<DropEvent> GetEventAsync(long eventId, long tenant, CancellationToken ct);
    Task<List<QueueEntry>> ListExpiredEntriesAsync(long eventId, CancellationToken ct);
    Task<int> ActiveSessionsAsync(long eventId, DateTime now, CancellationToken ct);
    Task<List<QueueEntry>> ListWaitingEntriesAsync(long eventId, int slots, CancellationToken ct);
    Task<bool> IsEligibleAsync(long eventId, long customer, CancellationToken ct);
    Task<List<DropReservation>> ListExpiredReservationsAsync(long eventId, long tenant, DateTime now, CancellationToken ct);
    Task<List<DropOrder>> ListExpiredOrdersAsync(List<long> expiredIds, long tenant, CancellationToken ct);
    Task<List<QueueSession>> ListExpiredSessionsAsync(long eventId, long tenant, DateTime now, CancellationToken ct);
    Task<int?> TailAsync(long eventId, CancellationToken ct);
    Task<bool> HasProductsAsync(long eventId, long tenant, CancellationToken ct);
    Task<List<DropQueueEntryResponse>> ListCustomerEntriesAsync(CancellationToken ct);
}
