using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class PlatformBillingRepository(StoreCommerceDbContext db,
    IEntityRepository<PlatformSubscription> subscriptions, IEntityRepository<PlatformBillingInvoice> invoices,
    IEntityRepository<EnterpriseContractChange> offers) : IPlatformBillingRepository
{
    public IEntityRepository<PlatformSubscription> Subscriptions => subscriptions;
    public IEntityRepository<PlatformBillingInvoice> Invoices => invoices;
    public IEntityRepository<EnterpriseContractChange> Offers => offers;
    public IReadOnlyCollection<PlatformBillingInvoice> TrackedInvoices =>
        db.ChangeTracker.Entries<PlatformBillingInvoice>().Select(Entry => Entry.Entity).ToArray();

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }

    public async Task<IReadOnlyList<PlatformBillingInvoiceResponse>> ListInvoicesAsync(long tenant, long? beforeId, CancellationToken ct)
    {
        return await db.Set<PlatformBillingInvoice>().IgnoreQueryFilters().AsNoTracking()
            .Where(Invoice => Invoice.EnterpriseId == tenant && (!beforeId.HasValue || Invoice.Id < beforeId.Value))
            .OrderByDescending(Invoice => Invoice.Id).Take(50).Select(Invoice => new PlatformBillingInvoiceResponse(
                Invoice.Id, Invoice.OfferId, Invoice.Amount, Invoice.MonthlyAmount, Invoice.Currency,
                Invoice.IsUpgrade, Invoice.PeriodStart, Invoice.PeriodEnd, Invoice.Status, Invoice.AppliedAt, Invoice.ReviewCode)).ToListAsync(ct);
    }

    public Task<List<PlatformBillingKey>> DueAsync(DateTime now, CancellationToken ct)
    {
        return db.Set<PlatformSubscription>().IgnoreQueryFilters().AsNoTracking()
            .Where(Subscription => Subscription.ClosedAt == null && Subscription.NextCheckAt <= now)
            .OrderBy(Subscription => Subscription.NextCheckAt).ThenBy(Subscription => Subscription.Id).Take(50)
            .Select(Subscription => new PlatformBillingKey(Subscription.EnterpriseId, Subscription.Id)).ToListAsync(ct);
    }

    public Task<PlatformSubscription?> LatestSubscriptionAsync(long tenant, CancellationToken ct)
    {
        return db.Set<PlatformSubscription>().IgnoreQueryFilters().AsNoTracking()
            .Where(Subscription => Subscription.EnterpriseId == tenant)
            .OrderByDescending(Subscription => Subscription.Id).FirstOrDefaultAsync(ct);
    }

    public Task<string?> CheckoutUrlAsync(long subscriptionId, long? offerId, DateTime now, CancellationToken ct)
    {
        return db.Set<PlatformBillingInvoice>().IgnoreQueryFilters().AsNoTracking().Where(Invoice =>
                Invoice.SubscriptionId == subscriptionId && Invoice.OfferId == offerId && Invoice.IsUpgrade &&
                Invoice.CheckoutExpiresAt > now && Invoice.Status != "approved" && Invoice.ReviewCode == null)
            .Select(Invoice => Invoice.CheckoutUrl).SingleOrDefaultAsync(ct);
    }
}
