using DropCommerce.Application.Interfaces.Queue;
using Microsoft.EntityFrameworkCore;
using DropCommerce.Domain.Entity;
using DropCommerce.Application.Contracts;
using DropCommerce.Infrastructure.Data.Context;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropQueueRepository(DropCommerceDbContext db) : DropCommercePersistence(db), IDropQueueRepository
{
    public Task<DropEvent?> FindEventAsync(long eventId, CancellationToken ct)
    {
        return Context.DropEvents.SingleOrDefaultAsync(Event => Event.Id == eventId, ct);
    }

    public Task<bool> IsRegisteredAsync(long eventId, long customer, CancellationToken ct)
    {
        return Context.DropRegistrations.AnyAsync(Registration => Registration.DropEventId == eventId &&
                Registration.CustomerId == customer && Registration.IsEligible, ct);
    }

    public Task<QueueEntry?> FindEntryAsync(long eventId, long customer, CancellationToken ct)
    {
        return Context.QueueEntries.IgnoreQueryFilters().SingleOrDefaultAsync(Entry =>
                Entry.DropEventId == eventId && Entry.CustomerId == customer, ct);
    }

    public Task<QueueEntry?> FindCustomerEntryAsync(long eventId, long customer, long tenant, CancellationToken ct)
    {
        return Context.QueueEntries.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(Entry =>
            Entry.DropEventId == eventId && Entry.CustomerId == customer && Entry.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropQueueSessionResponse?> FindActiveSessionAsync(long entryId, long customer, DateTime now, CancellationToken ct)
    {
        return Context.QueueSessions.IgnoreQueryFilters().AsNoTracking().Where(Session => Session.QueueEntryId == entryId &&
            Session.CustomerId == customer && Session.QueueSessionStatusId == 1 && Session.ExpiresAt > now)
            .OrderByDescending(Session => Session.Id).Select(Session => new DropQueueSessionResponse(Session.Id, Session.IssuedAt, Session.ExpiresAt)).FirstOrDefaultAsync(ct);
    }

    public Task<QueueSession?> FindSessionAsync(long sessionId, long customer, long eventId, long tenant, CancellationToken ct)
    {
        return Context.QueueSessions.IgnoreQueryFilters().Include(Session => Session.QueueEntry)
            .ThenInclude(Entry => Entry.DropEvent).SingleOrDefaultAsync(Session => Session.Id == sessionId && Session.CustomerId == customer &&
                Session.QueueEntry.DropEventId == eventId && Session.QueueEntry.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropReservation?> FindReservationAsync(long sessionId, long customer, string key, CancellationToken ct)
    {
        return Context.DropReservations.IgnoreQueryFilters().Include(Reservation => Reservation.Lines)
            .SingleOrDefaultAsync(Reservation => Reservation.QueueSessionId == sessionId && Reservation.CustomerId == customer &&
                Reservation.IdempotencyKey == key, ct);
    }

    public Task<DropProduct?> FindParticipationAsync(long participationId, long eventId, long tenant, CancellationToken ct)
    {
        return Context.DropProducts.IgnoreQueryFilters().Include(Product => Product.CatalogProduct).ThenInclude(Product => Product!.Variants)
            .SingleOrDefaultAsync(Product => Product.Id == participationId && Product.DropEventId == eventId &&
                Product.DropEvent.EnterpriseId == tenant && !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked, ct);
    }

    public Task<long> ReservedUnitsAsync(long eventId, long productId, string identityHash, DateTime now, CancellationToken ct)
    {
        return Context.DropReservations.IgnoreQueryFilters().Where(Reservation => Reservation.DropEventId == eventId &&
            Reservation.DropProduct.CatalogProductId == productId && Reservation.PurchaseIdentityHash == identityHash &&
            (Reservation.DropReservationStatusId == 2 || Reservation.DropReservationStatusId == 1 && Reservation.ExpiresAt > now))
            .SumAsync(Reservation => (long)Reservation.Quantity, ct);
    }

    public Task<int> PurchaseLimitAsync(long eventId, long productId, long tenant, CancellationToken ct)
    {
        return Context.DropProducts.IgnoreQueryFilters().Where(Product => Product.DropEventId == eventId &&
            Product.CatalogProductId == productId && Product.DropEvent.EnterpriseId == tenant && Product.IsActive && !Product.IsDeleted)
            .MinAsync(Product => Product.MaxPerCustomer, ct);
    }

    public Task<DropEvent> GetEventAsync(long eventId, long tenant, CancellationToken ct)
    {
        return Context.DropEvents.IgnoreQueryFilters().SingleAsync(Event => Event.Id == eventId && Event.EnterpriseId == tenant, ct);
    }

    public Task<List<QueueEntry>> ListExpiredEntriesAsync(long eventId, CancellationToken ct)
    {
        return Context.QueueEntries.IgnoreQueryFilters().Where(Entry => Entry.DropEventId == eventId && Entry.QueueEntryStatusId == 5 && Entry.PurchaseIdentityHash != null)
            .OrderBy(Entry => Entry.ExpiredAt).ThenBy(Entry => Entry.Id).ToListAsync(ct);
    }

    public Task<int> ActiveSessionsAsync(long eventId, DateTime now, CancellationToken ct)
    {
        return Context.QueueSessions.IgnoreQueryFilters().CountAsync(Session => Session.QueueEntry.DropEventId == eventId &&
            Session.QueueSessionStatusId == 1 && Session.ExpiresAt > now, ct);
    }

    public Task<List<QueueEntry>> ListWaitingEntriesAsync(long eventId, int slots, CancellationToken ct)
    {
        return Context.QueueEntries.IgnoreQueryFilters().Where(Entry => Entry.DropEventId == eventId && Entry.QueueEntryStatusId == 1 && Entry.PurchaseIdentityHash != null)
            .OrderBy(Entry => Entry.Position).ThenBy(Entry => Entry.Id).Take(slots).ToListAsync(ct);
    }

    public Task<bool> IsEligibleAsync(long eventId, long customer, CancellationToken ct)
    {
        return Context.DropRegistrations.IgnoreQueryFilters().AnyAsync(Registration =>
                Registration.DropEventId == eventId && Registration.CustomerId == customer && Registration.IsEligible && !Registration.IsDeleted, ct);
    }

    public Task<List<DropReservation>> ListExpiredReservationsAsync(long eventId, long tenant, DateTime now, CancellationToken ct)
    {
        return Context.DropReservations.IgnoreQueryFilters().Include(Reservation => Reservation.Lines).ThenInclude(Line => Line.Variant)
            .Where(Reservation => Reservation.DropEventId == eventId && Reservation.DropEvent.EnterpriseId == tenant &&
                Reservation.DropReservationStatusId == 1 && Reservation.ExpiresAt <= now).ToListAsync(ct);
    }

    public Task<List<DropOrder>> ListExpiredOrdersAsync(List<long> expiredIds, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().Where(Order => expiredIds.Contains(Order.DropReservationId) &&
            Order.DropEvent.EnterpriseId == tenant && Order.DropOrderPaymentStatusId == 1).ToListAsync(ct);
    }

    public Task<List<QueueSession>> ListExpiredSessionsAsync(long eventId, long tenant, DateTime now, CancellationToken ct)
    {
        return Context.QueueSessions.IgnoreQueryFilters().Include(Session => Session.QueueEntry)
            .Where(Session => Session.QueueEntry.DropEventId == eventId && Session.QueueEntry.DropEvent.EnterpriseId == tenant &&
                Session.QueueSessionStatusId == 1 && Session.ExpiresAt <= now).ToListAsync(ct);
    }

    public Task<int?> TailAsync(long eventId, CancellationToken ct)
    {
        return Context.QueueEntries.IgnoreQueryFilters().Where(Entry => Entry.DropEventId == eventId)
            .Select(Entry => (int?)Entry.Position).MaxAsync(ct);
    }

    public Task<bool> HasProductsAsync(long eventId, long tenant, CancellationToken ct)
    {
        return Context.DropProducts.IgnoreQueryFilters().AnyAsync(Product => Product.DropEventId == eventId && Product.DropEvent.EnterpriseId == tenant &&
            !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked && Product.CatalogProduct != null &&
            !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked && !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
            (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null) &&
            Product.CatalogProduct.Variants.Any(Variant => Variant.QuantityOnHand > Variant.QuantityReserved), ct);
    }

    public Task<List<DropQueueEntryResponse>> ListCustomerEntriesAsync(CancellationToken ct)
    {
        return Context.QueueEntries.AsNoTracking().Select(Entry => new DropQueueEntryResponse(
            Entry.Id, Entry.DropEventId, Entry.Position, Entry.QueueEntryStatusId, Entry.EnteredAt)).ToListAsync(ct);
    }
}
