using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Enums.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class OrderPaymentOperationWriter(
    IPaymentOperationStore operations)
{
    public async Task EnqueueRefundsAsync(Order order, string reason, OrderChangeSource source, long? actorId,
            DateTime now, CancellationToken ct)
    {
        var captured = order.ListTransaction.Where(Transaction => Transaction.IsCapturedPayment && Transaction.Amount > 0).ToList();

        foreach (var payment in captured)
            await operations.EnqueueAsync(order, payment, PaymentOperationKind.FullRefund, reason, source, actorId, now, ct);

        // Legados sem lançamento precisam de revisão; nunca afirmar um estorno que não foi comprovado.
        if (captured.Count == 0 && order.TotalAmount > 0)
            await operations.EnqueueAsync(order, null, PaymentOperationKind.FullRefund, reason, source, actorId, now, ct);
    }

    public async Task EnqueueCancellationsAsync(Order order, string reason, OrderChangeSource source, long? actorId,
            DateTime now, CancellationToken ct)
    {
        foreach (var payment in order.ListTransaction.Where(Transaction => Transaction.IsCancellablePayment).ToList())
        {
            await operations.EnqueueAsync(order, payment, PaymentOperationKind.Cancel, reason, source, actorId, now, ct);
            payment.MarkCancelled();
        }
    }
}
