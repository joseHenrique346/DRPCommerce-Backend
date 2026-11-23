using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Interfaces.Reconciliation;
using DropCommerce.Domain.Entity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropReconciliationReader(DropCommerceDbContext db) : IDropReconciliationReader
{
    public Task<List<DropQueueWorkKey>> ListQueueWorkAsync(long cursor, DateTime now, CancellationToken ct)
    {
        return db.DropEvents.IgnoreQueryFilters().Where(Event => Event.Id > cursor &&
            (Event.DropEndsAt > now && db.QueueEntries.IgnoreQueryFilters().Any(Entry => Entry.DropEventId == Event.Id &&
                (Entry.QueueEntryStatusId == 1 || Entry.QueueEntryStatusId == 2 || Entry.QueueEntryStatusId == 5)) ||
            db.QueueSessions.IgnoreQueryFilters().Any(Session => Session.QueueEntry.DropEventId == Event.Id && Session.QueueSessionStatusId == 1) ||
            db.DropReservations.IgnoreQueryFilters().Any(Reservation => Reservation.DropEventId == Event.Id && Reservation.DropReservationStatusId == 1)))
            .OrderBy(Event => Event.Id).Take(100).Select(Event => new DropQueueWorkKey(Event.EnterpriseId, Event.Id)).ToListAsync(ct);
    }

    public Task<List<string>> ListPendingStockReturnSnapshotsAsync(CancellationToken ct)
    {
        return db.Set<DropStockReturnIntent>().IgnoreQueryFilters().AsNoTracking().Where(Return => Return.CompletedAt == null)
            .OrderBy(Return => Return.Id).Take(50).Select(Return => Return.SnapshotJson).ToListAsync(ct);
    }

    public Task<List<DropPendingOrder>> ListUnfinishedOrdersAsync(long orderCursor, CancellationToken ct)
    {
        return db.DropOrders.IgnoreQueryFilters().AsNoTracking()
            .Where(Order => Order.Id > orderCursor && Order.PaymentOperationId != null && (!Order.PaymentIntentAccepted ||
                Order.DropOrderStatusId >= 5 && !Order.FulfillmentAcknowledged))
            .OrderBy(Order => Order.Id).Take(50).Select(Order => new DropPendingOrder(Order.Id, Order.DropEvent.EnterpriseId, Order.PaymentIntentAccepted)).ToListAsync(ct);
    }
}
