namespace StoreCommerce.Application.Payments;

public enum VerifiedPaymentStatus
{
    Pending = 1,
    Approved = 2,
    Authorized = 3,
    Rejected = 4,
    Cancelled = 5,
    Refunded = 6
}

public sealed record VerifiedPaymentUpdate(ExternalPaymentOwner Owner, VerifiedExternalPayment Payment,
    long ProviderConnectionId, VerifiedPaymentStatus Status, decimal RefundedAmount, DateTime ProviderUpdatedAt, long? MethodId = null);
