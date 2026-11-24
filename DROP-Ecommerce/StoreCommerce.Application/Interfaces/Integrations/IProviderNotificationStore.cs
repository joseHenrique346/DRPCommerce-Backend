using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Interfaces.Integrations;

public interface IProviderNotificationStore
{
    Task<List<ProviderNotificationSummary>> ListPendingAsync(long enterpriseId, CancellationToken ct);
    Task<ProviderConnection?> FindConnectionAsync(long connectionId, CancellationToken ct);
    Task<int> ClaimNotificationAsync(long enterpriseId, long id, DateTime now, Guid lease, CancellationToken ct);
    Task<ProviderNotification> GetNotificationAsync(long enterpriseId, long id, CancellationToken ct);
    Task<int> CompleteNotificationLeaseAsync(long enterpriseId, long id, Guid lease, ProviderNotificationStatus status, string code, DateTime nextAttemptAt, CancellationToken ct);
    Task<PaymentAttempt?> FindPaymentAttemptAsync(ProviderConnection connection, string externalReference, CancellationToken ct);
    Task<DropSalePaymentIntent?> FindDropPaymentIntentAsync(ProviderConnection connection, string externalReference, CancellationToken ct);
    Task<int> LinkPaymentAttemptAsync(ProviderConnection connection, string paymentId, long attemptId, CancellationToken ct);
    Task<int> CompleteReviewedNotificationsAsync(ProviderConnection connection, string paymentId, CancellationToken ct);
    Task<int> ClaimPaymentCheckAsync(long enterpriseId, long attemptId, DateTime now, CancellationToken ct);
    Task<PaymentAttempt> GetPaymentAttemptAsync(long enterpriseId, long attemptId, CancellationToken ct);
    Task<int> MarkDeliveryForReviewAsync(long enterpriseId, long connectionId, string delivery, CancellationToken ct);
    Task<List<MercadoPagoNotificationKey>> ReadDueAsync(DateTime now, CancellationToken ct);
    Task<List<MercadoPagoNotificationKey>> ReadDuePaymentsAsync(DateTime now, CancellationToken ct);
    Task<int> ClearExpiredAuthorizationsAsync(DateTime now, CancellationToken ct);
    void ClearTracking();
    Task InsertNotificationAsync(ProviderNotification notification, bool detachAfterInsert, CancellationToken ct);
}
