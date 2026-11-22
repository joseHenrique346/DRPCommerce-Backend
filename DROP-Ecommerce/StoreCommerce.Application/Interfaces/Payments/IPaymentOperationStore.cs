using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Enums.Payments;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.Payments;

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
