using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public enum PaymentOperationKind
{
    Cancel = 1,
    FullRefund = 2
}

public enum PaymentOperationStatus
{
    Requested = 1,
    Processing = 2,
    Pending = 3,
    Succeeded = 4,
    Failed = 5,
    OutcomeUnknown = 6,
    AwaitingManualConfirmation = 7,
    ReviewRequired = 8,
    Superseded = 9
}

/// <summary>
/// Intenção gravada junto com o cancelamento. Uma chamada interrompida só pode ser recuperada por consulta.
/// Conta, ambiente e chave não são substituídos pela conexão atual da loja.
/// </summary>
public sealed class PaymentOperation : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long OrderId { get; private set; }
    public long? PaymentTransactionId { get; private set; }
    public long? PaymentAttemptId { get; private set; }
    public long? ProviderConnectionId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public IntegrationEnvironment? Environment { get; private set; }
    public string? ExternalAccountId { get; private set; }
    public string? ExternalReference { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string? PaymentId { get; private set; }
    public string? PreferenceId { get; private set; }
    public decimal Amount { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public OrderChangeSource Source { get; private set; }
    public long? ActorId { get; private set; }
    public PaymentOperationKind Kind { get; private set; }
    public PaymentOperationStatus Status { get; private set; }
    public string? ResultReference { get; private set; }
    public string? ResolutionCode { get; private set; }
    public string? ManualConfirmationNote { get; private set; }
    public long? ConfirmedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime NextReconciliationAt { get; private set; }
    public Guid? LeaseToken { get; private set; }
    public DateTime? LeaseExpiresAt { get; private set; }
    public long Version { get; private set; }
    public Order Order { get; private set; } = null!;

    private PaymentOperation()
    {
    }

    public static PaymentOperation Create(Order order, Transaction? payment, PaymentAttempt? attempt,
        PaymentOperationKind kind, string reason, OrderChangeSource source, long? actorId, DateTime now)
    {
        if (!order.IsCancelled || string.IsNullOrWhiteSpace(reason) || reason.Length > 1000 ||
            kind is not (PaymentOperationKind.Cancel or PaymentOperationKind.FullRefund) ||
            payment is not null && (payment.OrderId != order.Id || payment.CustomerId != order.CustomerId) ||
            attempt is not null && (attempt.OrderId != order.Id || attempt.TransactionId != payment?.Id))
            throw new ArgumentException("Solicitação de pagamento inválida.");

        if (payment is not null && (kind == PaymentOperationKind.FullRefund && !payment.IsCapturedPayment ||
            kind == PaymentOperationKind.Cancel && !payment.IsCancellablePayment))
            throw new ArgumentException("O estado do pagamento não permite esta operação.");

        var manual = payment?.GatewayProvider == IntegrationProviders.Manual;

        return new PaymentOperation
        {
            EnterpriseId = order.EnterpriseId,
            OrderId = order.Id,
            Order = order,
            PaymentTransactionId = payment?.Id,
            PaymentAttemptId = attempt?.Id,
            ProviderConnectionId = attempt?.ProviderConnectionId,
            Provider = payment?.GatewayProvider ?? string.Empty,
            Environment = attempt?.Environment ?? (manual ? IntegrationEnvironment.Sandbox : null),
            ExternalAccountId = attempt?.ExternalAccountId ?? (manual ? $"manual-{order.EnterpriseId}" : null),
            ExternalReference = attempt?.ExternalReference,
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            Amount = payment?.Amount ?? order.TotalAmount,
            CurrencyCode = attempt?.CurrencyCode,
            Reason = reason,
            Kind = kind,
            Source = source,
            ActorId = actorId,
            Status = payment is null || !manual && attempt is null
                ? PaymentOperationStatus.ReviewRequired : PaymentOperationStatus.Requested,
            ResolutionCode = payment is null || !manual && attempt is null ? "MissingPaymentContext" : null,
            NextReconciliationAt = now
        };
    }

    public void SetTarget(string? paymentId, string? preferenceId)
    {
        if (paymentId?.Length > 200 || preferenceId?.Length > 200 ||
            PaymentId is not null && PaymentId != paymentId || PreferenceId is not null && PreferenceId != preferenceId)
            throw new ArgumentException("A identidade da cobrança não pode mudar.");

        PaymentId = paymentId;
        PreferenceId = preferenceId;
    }

    public void Record(PaymentOperationStatus status, string? reference, string? code, DateTime now)
    {
        Status = status;
        ResultReference = reference;
        ResolutionCode = code;
        LeaseToken = null;
        LeaseExpiresAt = null;
        NextReconciliationAt = now.AddMinutes(1);
        CompletedAt = status is PaymentOperationStatus.Succeeded or PaymentOperationStatus.Superseded ? now : null;
        Version++;
    }

    public void ConfirmManual(string reference, string note, long userId, DateTime now)
    {
        if (Provider != IntegrationProviders.Manual || Kind != PaymentOperationKind.FullRefund ||
            Status != PaymentOperationStatus.AwaitingManualConfirmation || string.IsNullOrWhiteSpace(reference) ||
            reference.Length > 200 || string.IsNullOrWhiteSpace(note) || note.Length > 1000)
            throw new ArgumentException("Confirme somente estornos manuais pendentes, informando referência e observação.");

        ConfirmedByUserId = userId;
        ManualConfirmationNote = note;
        Record(PaymentOperationStatus.Succeeded, reference, "ManuallyConfirmed", now);
    }

    public void Retry(DateTime now)
    {
        if (Status != PaymentOperationStatus.Failed)
            throw new ArgumentException("Somente falhas comprovadamente sem efeito aceitam nova tentativa.");

        Status = PaymentOperationStatus.Requested;
        ResolutionCode = null;
        NextReconciliationAt = now;
        Version++;
    }
}
