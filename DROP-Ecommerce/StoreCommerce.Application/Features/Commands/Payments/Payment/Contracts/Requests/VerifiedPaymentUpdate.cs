using StoreCommerce.Application.Enums.Payments;

namespace StoreCommerce.Application.Payments;

public sealed record VerifiedPaymentUpdate(ExternalPaymentOwner Owner, VerifiedExternalPayment Payment,
    long ProviderConnectionId, VerifiedPaymentStatus Status, decimal RefundedAmount, DateTime ProviderUpdatedAt, long? MethodId = null);
