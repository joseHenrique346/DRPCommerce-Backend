using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces;

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
    public PaymentAttempt PaymentAttempt { get; private set; } = null!;

    private PaymentExternalPayment()
    {
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
