using StoreCommerce.Domain.Enums.Payments;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentOperationRequest(long EnterpriseId, long OrderId, PaymentOperationKind Kind,
    PaymentProviderContext Context, string IdempotencyKey, string? ExternalReference, string? PaymentId,
    string? PreferenceId, decimal Amount, string? CurrencyCode, string Reason);
