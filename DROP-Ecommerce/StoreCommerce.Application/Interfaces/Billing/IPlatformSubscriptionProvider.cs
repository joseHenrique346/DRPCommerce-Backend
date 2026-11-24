using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Interfaces.Billing;

public interface IPlatformSubscriptionProvider
{
    Task<PlatformRecurringSubscription> CreateAsync(string reference, string payerEmail, decimal monthlyAmount, CancellationToken ct);
    Task<PlatformRecurringSubscription?> RecoverAsync(string reference, CancellationToken ct);
    Task<PlatformRecurringSubscription> GetAsync(string id, CancellationToken ct);
    Task<PlatformRecurringSubscription> UpdateAsync(string id, decimal monthlyAmount, bool cancel, CancellationToken ct);
    Task<IReadOnlyList<PlatformRecurringInvoice>> InvoicesAsync(string subscriptionId, CancellationToken ct);
    Task<PlatformRecurringInvoice> InvoiceAsync(string id, CancellationToken ct);
}
