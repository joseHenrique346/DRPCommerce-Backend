using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Enums.Payments;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Payments;

public sealed class PaymentOperationStore(StoreCommerceDbContext context, IProductRepository products,
    IOrderRepository orders, OrderWorkflowScope scope, TimeProvider time) : IPaymentOperationStore
{
    public async Task EnqueueAsync(Order order, Transaction? payment, PaymentOperationKind kind, string reason,
        OrderChangeSource source, long? actorId, DateTime now, CancellationToken ct)
    {
        if (context.Database.CurrentTransaction is null || scope.EnterpriseId != order.EnterpriseId)
            throw new InvalidOperationException("A solicitação exige a transação do pedido.");

        if (await context.PaymentOperations.IgnoreQueryFilters().AnyAsync(Operation =>
            Operation.EnterpriseId == order.EnterpriseId && Operation.OrderId == order.Id &&
            Operation.PaymentTransactionId == (payment == null ? null : payment.Id) && Operation.Kind == kind, ct))
            return;

        var attempt = payment is null ? null : await context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .Include(Attempt => Attempt.Payments)
            .SingleOrDefaultAsync(Attempt => Attempt.EnterpriseId == order.EnterpriseId &&
                (Attempt.TransactionId == payment.Id || Attempt.Payments.Any(Payment => Payment.CapturedTransactionId == payment.Id)), ct);
        var operation = PaymentOperation.Create(order, payment, attempt, kind, reason, source, actorId, now);
        context.PaymentOperations.Add(operation);

        if (kind == PaymentOperationKind.FullRefund && payment is not null)
        {
            // A captura confirmada substitui o cancelamento remoto ainda em andamento. O lease antigo perde validade.
            await context.PaymentOperations.IgnoreQueryFilters()
                .Where(Operation => Operation.EnterpriseId == order.EnterpriseId && Operation.OrderId == order.Id &&
                    Operation.PaymentTransactionId == payment.Id && Operation.Kind == PaymentOperationKind.Cancel &&
                    Operation.Status != PaymentOperationStatus.Succeeded && Operation.Status != PaymentOperationStatus.Superseded)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(Operation => Operation.Status, PaymentOperationStatus.Superseded)
                    .SetProperty(Operation => Operation.ResolutionCode, "PaymentCaptured")
                    .SetProperty(Operation => Operation.CompletedAt, now)
                    .SetProperty(Operation => Operation.LeaseToken, (Guid?)null)
                    .SetProperty(Operation => Operation.LeaseExpiresAt, (DateTime?)null)
                    .SetProperty(Operation => Operation.Version, Operation => Operation.Version + 1), ct);
        }

        // O CAS disputa com o claim de criação. Se o POST já começou, conserva a intenção para consulta e cancelamento.
        if (kind == PaymentOperationKind.Cancel && attempt is not null)
        {
            await context.PaymentAttempts.IgnoreQueryFilters()
                .Where(Attempt => Attempt.Id == attempt.Id && Attempt.EnterpriseId == order.EnterpriseId &&
                    Attempt.Status == PaymentCreationStatus.Requested && Attempt.LeaseToken == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(Attempt => Attempt.Status, PaymentCreationStatus.Aborted)
                    .SetProperty(Attempt => Attempt.Version, Attempt => Attempt.Version + 1), ct);
        }
    }

    public Task<List<PaymentOperationKey>> GetDueAsync(DateTime now, CancellationToken ct)
    {
        return context.PaymentOperations.IgnoreQueryFilters().AsNoTracking()
            .Where(Operation => (Operation.Status == PaymentOperationStatus.Requested ||
                Operation.Status == PaymentOperationStatus.Processing || Operation.Status == PaymentOperationStatus.Pending ||
                Operation.Status == PaymentOperationStatus.OutcomeUnknown) && Operation.NextReconciliationAt <= now &&
                (Operation.LeaseExpiresAt == null || Operation.LeaseExpiresAt <= now))
            .OrderBy(Operation => Operation.NextReconciliationAt).ThenBy(Operation => Operation.Id).Take(50)
            .Select(Operation => new PaymentOperationKey(Operation.EnterpriseId, Operation.Id)).ToListAsync(ct);
    }

    public async Task<PaymentOperationWork?> TryClaimAsync(long enterpriseId, long operationId, DateTime now, CancellationToken ct)
    {
        // Um processador não pode herdar a transação ou entidades antigas do cancelamento.
        if (context.Database.CurrentTransaction is not null || context.ChangeTracker.Entries().Any())
            throw new InvalidOperationException("Processe operações em um escopo independente, sem transação aberta.");

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        var operation = await context.PaymentOperations.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Operation => Operation.Id == operationId && Operation.EnterpriseId == enterpriseId, ct);

        if (operation is null || operation.NextReconciliationAt > now || operation.LeaseExpiresAt > now ||
            operation.Status is not (PaymentOperationStatus.Requested or PaymentOperationStatus.Processing or
                PaymentOperationStatus.Pending or PaymentOperationStatus.OutcomeUnknown))
            return null;

        var lease = Guid.NewGuid();
        var changed = await context.PaymentOperations.IgnoreQueryFilters()
            .Where(Operation => Operation.Id == operationId && Operation.EnterpriseId == enterpriseId &&
                Operation.Version == operation.Version && (Operation.LeaseExpiresAt == null || Operation.LeaseExpiresAt <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Operation => Operation.Status, PaymentOperationStatus.Processing)
                .SetProperty(Operation => Operation.LeaseToken, lease)
                .SetProperty(Operation => Operation.LeaseExpiresAt, now.AddMinutes(2))
                .SetProperty(Operation => Operation.Version, Operation => Operation.Version + 1), ct);

        if (changed == 0)
            return null;

        var recover = operation.Status != PaymentOperationStatus.Requested;

        if (operation.Provider != IntegrationProviders.Manual)
        {
            var attempt = await context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
                .Include(Attempt => Attempt.Payments)
                .SingleOrDefaultAsync(Attempt => Attempt.Id == operation.PaymentAttemptId && Attempt.EnterpriseId == enterpriseId, ct);

            if (attempt?.Status == PaymentCreationStatus.Aborted && operation.Kind == PaymentOperationKind.Cancel)
                await RecordWithoutCallAsync(operationId, lease, PaymentOperationStatus.Succeeded, "NotSubmitted", now, ct);
            else if (attempt is null || attempt.Provider != operation.Provider || attempt.Environment != operation.Environment ||
                attempt.ExternalAccountId != operation.ExternalAccountId)
                await RecordWithoutCallAsync(operationId, lease, PaymentOperationStatus.ReviewRequired, "MissingPaymentContext", now, ct);
            else if (attempt.Status != PaymentCreationStatus.Created && operation.Kind == PaymentOperationKind.Cancel)
                await RecordWithoutCallAsync(operationId, lease, PaymentOperationStatus.Requested, "AwaitingPaymentRecovery", now, ct);
            else
            {
                var paymentReference = await context.Transactions.IgnoreQueryFilters().AsNoTracking()
                    .Where(Transaction => Transaction.Id == operation.PaymentTransactionId && Transaction.OrderId == operation.OrderId &&
                        Transaction.Order.EnterpriseId == enterpriseId)
                    .Select(Transaction => Transaction.GatewayReference).SingleAsync(ct);
                // Uma preferência pode ter vários pagamentos: nunca adivinhar qual deles devolver.
                var paymentId = attempt.Payments.SingleOrDefault(Payment => Payment.PaymentId == paymentReference)?.PaymentId;

                if (operation.Kind == PaymentOperationKind.FullRefund && paymentId is null)
                    await RecordWithoutCallAsync(operationId, lease, PaymentOperationStatus.ReviewRequired, "MissingVerifiedPaymentId", now, ct);
                else
                {
                    operation.SetTarget(paymentId, attempt.PreferenceId);
                    await context.PaymentOperations.IgnoreQueryFilters()
                        .Where(Operation => Operation.Id == operationId && Operation.LeaseToken == lease)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(Operation => Operation.PaymentId, operation.PaymentId)
                            .SetProperty(Operation => Operation.PreferenceId, operation.PreferenceId), ct);
                    await transaction.CommitAsync(ct);

                    return new PaymentOperationWork(operation, lease, recover);
                }
            }

            await transaction.CommitAsync(ct);

            return null;
        }

        await transaction.CommitAsync(ct);

        return new PaymentOperationWork(operation, lease, recover);
    }

    private Task<int> RecordWithoutCallAsync(long id, Guid lease, PaymentOperationStatus status, string code, DateTime now, CancellationToken ct)
    {
        return context.PaymentOperations.IgnoreQueryFilters().Where(Operation => Operation.Id == id && Operation.LeaseToken == lease)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Operation => Operation.Status, status)
                .SetProperty(Operation => Operation.ResolutionCode, code)
                .SetProperty(Operation => Operation.CompletedAt, status == PaymentOperationStatus.Succeeded ? now : (DateTime?)null)
                .SetProperty(Operation => Operation.LeaseToken, (Guid?)null)
                .SetProperty(Operation => Operation.LeaseExpiresAt, (DateTime?)null)
                .SetProperty(Operation => Operation.NextReconciliationAt, now.AddMinutes(1)), ct);
    }

    public async Task CompleteAsync(PaymentOperationWork work, PaymentOperationResult result, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(work.Operation.EnterpriseId, ct);
        var order = await orders.GetForWorkflowAsync(work.Operation.EnterpriseId, work.Operation.OrderId, ct)
            ?? throw new InvalidOperationException("Pedido da operação inexistente.");
        var operation = await context.PaymentOperations.IgnoreQueryFilters()
            .SingleAsync(Operation => Operation.Id == work.Operation.Id && Operation.EnterpriseId == work.Operation.EnterpriseId, ct);

        if (operation.LeaseToken != work.LeaseToken)
            return;

        operation.Record(result.Status, result.Reference, result.Status.ToString(), time.GetUtcNow().UtcDateTime);

        using (scope.Begin(order.EnterpriseId, null))
        {
            ApplyRefund(order, operation);
            await context.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task ResolveAsync(long enterpriseId, long orderId, long operationId, long userId,
        string? reference, string? note, bool retry, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);
        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");
        var operation = await context.PaymentOperations.IgnoreQueryFilters()
            .SingleOrDefaultAsync(Operation => Operation.Id == operationId && Operation.EnterpriseId == enterpriseId && Operation.OrderId == orderId, ct)
            ?? throw new SecurityFailure(404, "Payment.OperationNotFound", "Operação não encontrada.");

        if (retry)
            operation.Retry(time.GetUtcNow().UtcDateTime);
        else
        {
            if (operation.Provider != IntegrationProviders.Manual || operation.Kind != PaymentOperationKind.FullRefund)
                throw new ArgumentException("Esta confirmação é exclusiva de estornos manuais.");

            if (operation.Status != PaymentOperationStatus.Succeeded)
                operation.ConfirmManual(reference!, note!, userId, time.GetUtcNow().UtcDateTime);
        }

        using (scope.Begin(enterpriseId, null))
        {
            ApplyRefund(order, operation);
            await context.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    private void ApplyRefund(Order order, PaymentOperation operation)
    {
        if (operation.Kind != PaymentOperationKind.FullRefund || operation.Status != PaymentOperationStatus.Succeeded)
            return;

        var payment = order.ListTransaction.Single(Transaction => Transaction.Id == operation.PaymentTransactionId);

        if (payment.IsCapturedPayment)
            context.Transactions.Add(Transaction.CreateRefund(payment, operation.ResultReference!, time.GetUtcNow().UtcDateTime));

        if (order.IsCancelled && !order.ListTransaction.Any(Transaction => Transaction.IsCapturedPayment))
            order.RegisterFullRefund(operation.Source, operation.ConfirmedByUserId ?? operation.ActorId,
                "Estorno integral confirmado", time.GetUtcNow().UtcDateTime);
    }
}
