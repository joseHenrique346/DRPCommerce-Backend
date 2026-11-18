using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Payments;

public interface IPaymentOperationStore
{
    Task EnqueueAsync(Order order, Transaction? payment, PaymentOperationKind kind, string reason,
        OrderChangeSource source, long? actorId, DateTime now, CancellationToken ct);
    Task<List<PaymentOperationKey>> GetDueAsync(DateTime now, CancellationToken ct);
    Task<PaymentOperationWork?> TryClaimAsync(long enterpriseId, long operationId, DateTime now, CancellationToken ct);
    Task CompleteAsync(PaymentOperationWork work, PaymentOperationResult result, CancellationToken ct);
    Task ResolveAsync(long enterpriseId, long orderId, long operationId, long userId,
        string? reference, string? note, bool retry, CancellationToken ct);
}

public sealed record PaymentOperationKey(long EnterpriseId, long OperationId);
public sealed record PaymentOperationWork(PaymentOperation Operation, Guid LeaseToken, bool RecoverOnly);

public sealed record PaymentOperationRequest(long EnterpriseId, long OrderId, PaymentOperationKind Kind,
    PaymentProviderContext Context, string IdempotencyKey, string? ExternalReference, string? PaymentId,
    string? PreferenceId, decimal Amount, string? CurrencyCode, string Reason);

/// <summary>
/// Failed só é permitido quando o provedor comprova que a operação não foi aplicada.
/// Succeeded exige uma referência verificável. Erros/transporte/timeout são OutcomeUnknown.
/// </summary>
public sealed record PaymentOperationResult(PaymentOperationStatus Status, string? Reference = null);
