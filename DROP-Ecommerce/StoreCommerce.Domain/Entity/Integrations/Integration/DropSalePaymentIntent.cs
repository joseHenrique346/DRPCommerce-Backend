using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Domain.Entity;

public sealed class DropSalePaymentIntent : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public Guid OperationId { get; private set; }
    public long DropOrderId { get; private set; }
    public long CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public DateTime ExpiresAt { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public string ExternalAccountId { get; private set; } = string.Empty;
    public long? ProviderConnectionId { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public DateTime? SubmittedAt { get; private set; }
    public string? PreferenceId { get; private set; }
    public string? CheckoutUrl { get; private set; }
    public string? PaymentId { get; private set; }
    public string Status { get; private set; } = "pending";
    public long Revision { get; private set; } = 1;
    public long AcknowledgedRevision { get; private set; }
    public DateTime NextCheckAt { get; private set; }
    public bool FulfillmentClosed { get; private set; }
    public bool FulfillmentReviewRequired { get; private set; }

    public void RequireFulfillmentReview()
    {
        FulfillmentReviewRequired = true;
        FulfillmentClosed = false;
    }

    public void CloseFulfillment()
    {
        if (!FulfillmentReviewRequired)
            FulfillmentClosed = true;
    }

    private DropSalePaymentIntent()
    {
    }

    public static DropSalePaymentIntent Prepare(long tenant, Guid operationId, long orderId, long customerId, decimal amount,
        DateTime expires, string provider, IntegrationEnvironment environment, string account, long? connection, DateTime now)
    {
        return new DropSalePaymentIntent
        {
            EnterpriseId = tenant, OperationId = operationId, DropOrderId = orderId, CustomerId = customerId,
            Amount = amount, ExpiresAt = expires, Provider = provider, Environment = environment,
            ExternalAccountId = account, ProviderConnectionId = connection, ExternalReference = "drp-drop-" + operationId.ToString("N"), NextCheckAt = now
        };
    }

    public void Submit(DateTime now)
    {
        if (SubmittedAt.HasValue)
            throw new InvalidOperationException("A criação já foi submetida.");

        SubmittedAt = now;
    }

    public void Checkout(string preferenceId, string? url)
    {
        if (PreferenceId is not null && PreferenceId != preferenceId)
            throw new InvalidOperationException("Preferência financeira divergente.");

        if (PreferenceId is null)
            Revision = checked(Revision + 1);

        PreferenceId = preferenceId;
        CheckoutUrl = url;
    }

    public void Observe(string status, string? paymentId)
    {
        if (Status == status && PaymentId == paymentId)
            return;

        if (Status == "approved" && status is "pending" or "rejected")
            return;

        Status = status;
        PaymentId = paymentId;
        Revision = checked(Revision + 1);
    }

    public void Acknowledge(long revision)
    {
        if (revision > Revision || revision < 1)
            throw new ArgumentException("Revisão de pagamento inválida.");

        AcknowledgedRevision = Math.Max(AcknowledgedRevision, revision);
    }

    public void CheckAfter(DateTime next)
    {
        NextCheckAt = next;
    }
}
