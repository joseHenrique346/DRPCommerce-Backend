namespace StoreCommerce.Application.Billing;

public sealed record PlatformRecurringInvoice(string Id, string SubscriptionId, decimal Amount,
    string Currency, DateTime DebitDate, string? PaymentId);
