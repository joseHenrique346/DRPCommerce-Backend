using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Um checkout/preferência pode originar vários pagamentos. Cada ID só pode pertencer a uma tentativa na conta/ambiente.
/// </summary>
public sealed class PaymentExternalPayment : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long PaymentAttemptId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public string ExternalAccountId { get; private set; } = string.Empty;
    public string PaymentId { get; private set; } = string.Empty;
    public long? CapturedTransactionId { get; private set; }
    public Transaction? CapturedTransaction { get; private set; }
    public DateTime? ProviderUpdatedAt { get; private set; }
    public PaymentAttempt PaymentAttempt { get; private set; } = null!;

    private PaymentExternalPayment()
    {
    }

    public void BindCapture(Transaction transaction)
    {
        if (CapturedTransactionId.HasValue && CapturedTransactionId != transaction.Id ||
            transaction.OrderId != PaymentAttempt.OrderId || transaction.GatewayProvider != Provider ||
            transaction.GatewayReference != PaymentId || transaction.Amount != PaymentAttempt.Amount)
            throw new ArgumentException("O lançamento não corresponde ao pagamento externo.");

        CapturedTransaction = transaction;
        CapturedTransactionId = transaction.Id;
    }

    public void RecordProviderUpdate(DateTime updatedAt)
    {
        if (ProviderUpdatedAt is null || ProviderUpdatedAt < updatedAt)
            ProviderUpdatedAt = updatedAt;
    }

    public static PaymentExternalPayment Create(PaymentAttempt attempt, string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId) || paymentId.Length > 200)
            throw new ArgumentException("Identificador externo de pagamento inválido.");

        return new PaymentExternalPayment
        {
            PaymentAttemptId = attempt.Id,
            EnterpriseId = attempt.EnterpriseId,
            Provider = attempt.Provider,
            Environment = attempt.Environment,
            ExternalAccountId = attempt.ExternalAccountId,
            PaymentId = paymentId
        };
    }
}
