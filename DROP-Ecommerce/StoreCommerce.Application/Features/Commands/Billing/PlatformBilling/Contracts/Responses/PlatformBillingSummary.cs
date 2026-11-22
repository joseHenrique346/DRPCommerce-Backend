namespace StoreCommerce.Application.Contracts;

public sealed record PlatformBillingSummary(long? SubscriptionId, string Status, decimal MonthlyAmount,
    DateTime? PaidUntil, DateTime? GraceEndsAt, string? CheckoutUrl, long? PendingChangeId,
    DateTime? ChangeEffectiveAt, IReadOnlyList<PlatformBillingNotice> Notices);
