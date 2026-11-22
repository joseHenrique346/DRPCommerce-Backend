using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Billing;

public interface IPlatformBillingNotificationRepository
{
    IEntityRepository<PlatformBillingNotification> Notifications { get; }
    IEntityRepository<PlatformSubscription> Subscriptions { get; }
    IEntityRepository<PlatformBillingInvoice> Invoices { get; }
    Task AddDeliveryAsync(PlatformBillingNotification notification, CancellationToken ct);
    Task<List<long>> DueAsync(DateTime now, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
    void ClearTracking();
}
