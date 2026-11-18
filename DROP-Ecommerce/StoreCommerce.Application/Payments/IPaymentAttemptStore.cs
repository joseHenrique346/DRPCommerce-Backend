using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentAttemptWork(PaymentAttempt Attempt, Guid LeaseToken, bool RecoverOnly);
public sealed record PaymentAttemptKey(long EnterpriseId, long AttemptId);
public sealed record VerifiedExternalPayment(long EnterpriseId, string Provider, IntegrationEnvironment Environment,
    string ExternalAccountId, string ExternalReference, string PaymentId, decimal Amount, string CurrencyCode);
public sealed record ExternalPaymentOwner(long EnterpriseId, long OrderId, long AttemptId);

public interface IPaymentAttemptStore
{
    void Add(PaymentAttempt attempt);
    Task EnsureLegacyAttemptAsync(long enterpriseId, long orderId, string currencyCode, CancellationToken ct);
    Task<long?> FindForOrderAsync(long enterpriseId, long orderId, CancellationToken ct);
    Task<List<PaymentAttemptKey>> GetDueAsync(DateTime now, int take, CancellationToken ct);
    Task<PaymentAttemptWork?> TryClaimAsync(long enterpriseId, long attemptId, DateTime now, CancellationToken ct);
    Task CompleteAsync(PaymentAttemptWork work, PaymentSession session, CancellationToken ct);
    Task MarkUnknownAsync(PaymentAttemptWork work, DateTime nextReconciliationAt, CancellationToken ct);
    Task MarkNotSubmittedAsync(PaymentAttemptWork work, DateTime nextAttemptAt, CancellationToken ct);
    /// <summary>
    /// Chamado pelo adapter após consultar o provedor. Valida o escopo/valor e vincula um PaymentId sem confirmar a venda.
    /// </summary>
    Task<ExternalPaymentOwner> RegisterVerifiedPaymentAsync(VerifiedExternalPayment payment, CancellationToken ct);
}
