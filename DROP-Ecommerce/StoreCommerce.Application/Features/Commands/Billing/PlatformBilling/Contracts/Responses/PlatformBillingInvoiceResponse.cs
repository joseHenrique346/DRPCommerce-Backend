namespace StoreCommerce.Application.Contracts;

public sealed record PlatformBillingInvoiceResponse(long Id, long OfferId, decimal Amount, decimal MonthlyAmount,
    string Currency, bool IsUpgrade, DateTime PeriodStart, DateTime PeriodEnd, string Status,
    DateTime? AppliedAt, string? ReviewCode);
