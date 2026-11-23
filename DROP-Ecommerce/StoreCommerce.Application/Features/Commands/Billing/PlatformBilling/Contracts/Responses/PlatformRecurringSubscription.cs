namespace StoreCommerce.Application.Billing;

public sealed record PlatformRecurringSubscription(string Id, string Reference, string AccountId,
    decimal MonthlyAmount, string Currency, string Status, string? CheckoutUrl);
