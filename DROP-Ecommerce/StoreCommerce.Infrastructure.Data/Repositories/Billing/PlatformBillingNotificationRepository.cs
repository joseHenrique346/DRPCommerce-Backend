using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Billing;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class PlatformBillingNotificationRepository(StoreCommerceDbContext db,
    IEntityRepository<PlatformBillingNotification> notifications,
    IEntityRepository<PlatformSubscription> subscriptions,
    IEntityRepository<PlatformBillingInvoice> invoices) : IPlatformBillingNotificationRepository
{
    public IEntityRepository<PlatformBillingNotification> Notifications => notifications;
    public IEntityRepository<PlatformSubscription> Subscriptions => subscriptions;
    public IEntityRepository<PlatformBillingInvoice> Invoices => invoices;

    public async Task AddDeliveryAsync(PlatformBillingNotification notification, CancellationToken ct)
    {
        db.Add(notification);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // The signed delivery is already durable; grants are also idempotent.
        }
        finally
        {
            db.Entry(notification).State = EntityState.Detached;
        }
    }

    public Task<List<long>> DueAsync(DateTime now, CancellationToken ct)
    {
        return db.Set<PlatformBillingNotification>().AsNoTracking().Where(Notification =>
            Notification.CompletedAt == null && Notification.NextCheckAt <= now)
            .OrderBy(Notification => Notification.NextCheckAt).ThenBy(Notification => Notification.Id)
            .Take(50).Select(Notification => Notification.Id).ToListAsync(ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return db.SaveChangesAsync(ct);
    }

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }
}
