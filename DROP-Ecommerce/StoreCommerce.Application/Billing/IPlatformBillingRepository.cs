using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Billing;

public interface IPlatformBillingRepository
{
    IEntityRepository<PlatformSubscription> Subscriptions { get; }
    IEntityRepository<PlatformBillingInvoice> Invoices { get; }
    IEntityRepository<EnterpriseContractChange> Offers { get; }
    IReadOnlyCollection<PlatformBillingInvoice> TrackedInvoices { get; }
    void ClearTracking();
    Task<IReadOnlyList<PlatformBillingInvoiceResponse>> ListInvoicesAsync(long tenant, long? beforeId, CancellationToken ct);
    Task<List<PlatformBillingKey>> DueAsync(DateTime now, CancellationToken ct);
    Task<PlatformSubscription?> LatestSubscriptionAsync(long tenant, CancellationToken ct);
    Task<string?> CheckoutUrlAsync(long subscriptionId, long? offerId, DateTime now, CancellationToken ct);
}
