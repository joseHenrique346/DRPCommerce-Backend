using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public enum PaymentCreationStatus
{
    Requested = 1,
    Processing = 2,
    Created = 3,
    OutcomeUnknown = 4,
    Aborted = 5
}

/// <summary>
/// Intenção durável anterior à chamada externa. Moeda, valor, recebedor e correlação ficam congelados.
/// Processing abandonado exige consulta; nunca autoriza outro POST automaticamente.
/// </summary>
public sealed class PaymentAttempt : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long OrderId { get; private set; }
    public long TransactionId { get; private set; }
    public long? ProviderConnectionId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public string ExternalAccountId { get; private set; } = string.Empty;
    public string ExternalReference { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public long CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public long TransactionMethodId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public PaymentCreationStatus Status { get; private set; }
    public string? PreferenceId { get; private set; }
    public string? CheckoutUrl { get; private set; }
    public string? Instructions { get; private set; }
    public Guid? LeaseToken { get; private set; }
    public DateTime? LeaseExpiresAt { get; private set; }
    public DateTime NextReconciliationAt { get; private set; }
    public DateTime NextStatusCheckAt { get; private set; }
    public long Version { get; private set; }
    public Order Order { get; private set; } = null!;
    public Transaction Transaction { get; private set; } = null!;
    public ProviderConnection? ProviderConnection { get; private set; }
    public ICollection<PaymentExternalPayment> Payments { get; private set; } = [];

    private PaymentAttempt()
    {
    }

    public static PaymentAttempt Create(Transaction transaction, string provider, IntegrationEnvironment environment,
        string accountId, long? connectionId, string currencyCode, string customerName, string customerEmail,
        DateTime expiresAt, DateTime now)
    {
        IntegrationProviders.Validate(provider, environment);

        if (!transaction.IsPendingPayment || transaction.Amount <= 0 || transaction.GatewayProvider != provider ||
            string.IsNullOrWhiteSpace(accountId) || accountId.Length > 200 || expiresAt <= now)
            throw new ArgumentException("Tentativa de pagamento inválida.");

        if (provider != IntegrationProviders.Manual && connectionId is not > 0)
            throw new ArgumentException("Pagamento externo exige conexão autorizada da loja.");

        if (currencyCode.Length != 3 || currencyCode.Any(character => character is < 'A' or > 'Z') ||
            customerName.Length > 200 || customerEmail.Length > 255)
            throw new ArgumentException("Dados da tentativa de pagamento inválidos.");

        var correlation = Guid.NewGuid().ToString("N");

        return new PaymentAttempt
        {
            EnterpriseId = transaction.Order.EnterpriseId,
            Order = transaction.Order,
            OrderId = transaction.OrderId,
            Transaction = transaction,
            TransactionId = transaction.Id,
            ProviderConnectionId = connectionId,
            Provider = provider,
            Environment = environment,
            ExternalAccountId = accountId,
            ExternalReference = correlation,
            IdempotencyKey = correlation,
            Amount = transaction.Amount,
            CurrencyCode = currencyCode,
            CustomerId = transaction.CustomerId,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            TransactionMethodId = transaction.TransactionMethodId,
            ExpiresAt = expiresAt,
            Status = PaymentCreationStatus.Requested,
            NextReconciliationAt = now,
            NextStatusCheckAt = now
        };
    }
}
