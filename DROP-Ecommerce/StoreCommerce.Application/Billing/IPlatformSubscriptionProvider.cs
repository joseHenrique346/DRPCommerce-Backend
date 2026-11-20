namespace StoreCommerce.Application.Billing;

public sealed record PlatformRecurringSubscription(string Id, string Reference, string AccountId,
    decimal MonthlyAmount, string Currency, string Status, string? CheckoutUrl);

public sealed record PlatformRecurringInvoice(string Id, string SubscriptionId, decimal Amount,
    string Currency, DateTime DebitDate, string? PaymentId);

public interface IPlatformSubscriptionProvider
{
    Task<PlatformRecurringSubscription> CreateAsync(string reference, string payerEmail, decimal monthlyAmount, CancellationToken ct);
    Task<PlatformRecurringSubscription?> RecoverAsync(string reference, CancellationToken ct);
    Task<PlatformRecurringSubscription> GetAsync(string id, CancellationToken ct);
    Task<PlatformRecurringSubscription> UpdateAsync(string id, decimal monthlyAmount, bool cancel, CancellationToken ct);
    Task<IReadOnlyList<PlatformRecurringInvoice>> InvoicesAsync(string subscriptionId, CancellationToken ct);
    Task<PlatformRecurringInvoice> InvoiceAsync(string id, CancellationToken ct);
}
