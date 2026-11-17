using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Payments;

public sealed class PaymentAttemptStore(StoreCommerceDbContext context, IntegrationWriteScope scope, TimeProvider time) : IPaymentAttemptStore
{
    public void Add(PaymentAttempt attempt)
    {
        context.PaymentAttempts.Add(attempt);
    }

    public async Task EnsureLegacyAttemptAsync(long enterpriseId, long orderId, string currencyCode, CancellationToken ct)
    {
        // Só o adapter manual pode reconstruir uma operação antiga sem intenção persistida.
        // Para provedores reais a ausência de referência nunca prova que a cobrança não existe.
        var payment = await context.Transactions.IgnoreQueryFilters().Include(Transaction => Transaction.Order)
            .Include(Transaction => Transaction.Customer)
            .Where(Transaction => Transaction.Order.EnterpriseId == enterpriseId && Transaction.OrderId == orderId &&
                Transaction.TransactionTypeId == TransactionType.Payment.Id && Transaction.TransactionStatusId == TransactionStatus.Pending.Id &&
                Transaction.GatewayProvider == IntegrationProviders.Manual && Transaction.GatewayReference == string.Empty &&
                Transaction.Amount > 0)
            .SingleOrDefaultAsync(ct);

        if (payment is null || await FindForOrderAsync(enterpriseId, orderId, ct) is not null ||
            payment.Order.PaymentExpiresAt is not DateTime expires || expires <= time.GetUtcNow().UtcDateTime)
            return;

        var attempt = PaymentAttempt.Create(payment, IntegrationProviders.Manual, IntegrationEnvironment.Sandbox,
            $"manual-{enterpriseId}", null, currencyCode, payment.Customer.FullName, payment.Customer.Email.Value, expires, time.GetUtcNow().UtcDateTime);
        context.PaymentAttempts.Add(attempt);

        try
        {
            using (scope.Begin(enterpriseId))
                await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // Outra requisição criou a mesma intenção, protegida pelo índice único da transação.
        }
        finally
        {
            context.Entry(attempt).State = EntityState.Detached;
        }
    }

    public Task<long?> FindForOrderAsync(long enterpriseId, long orderId, CancellationToken ct)
    {
        return context.PaymentAttempts.IgnoreQueryFilters().Where(Attempt => Attempt.EnterpriseId == enterpriseId && Attempt.OrderId == orderId)
            .OrderByDescending(Attempt => Attempt.Id).Select(Attempt => (long?)Attempt.Id).FirstOrDefaultAsync(ct);
    }

    public Task<List<PaymentAttemptKey>> GetDueAsync(DateTime now, int take, CancellationToken ct)
    {
        return context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .Where(Attempt => Attempt.Status != PaymentCreationStatus.Created && Attempt.NextReconciliationAt <= now &&
                (Attempt.LeaseExpiresAt == null || Attempt.LeaseExpiresAt <= now) &&
                (Attempt.Status != PaymentCreationStatus.Requested ||
                    (Attempt.ExpiresAt > now && Attempt.Order.OrderStatusId == OrderStatus.Pending.Id && !Attempt.Order.IsDeleted)))
            .OrderBy(Attempt => Attempt.NextReconciliationAt).ThenBy(Attempt => Attempt.Id).Take(take)
            .Select(Attempt => new PaymentAttemptKey(Attempt.EnterpriseId, Attempt.Id)).ToListAsync(ct);
    }

    public async Task<PaymentAttemptWork?> TryClaimAsync(long enterpriseId, long attemptId, DateTime now, CancellationToken ct)
    {
        var attempt = await context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Attempt => Attempt.Id == attemptId && Attempt.EnterpriseId == enterpriseId, ct);

        if (attempt is null || attempt.Status == PaymentCreationStatus.Created || attempt.NextReconciliationAt > now || attempt.LeaseExpiresAt > now)
            return null;

        var lease = Guid.NewGuid();
        var changed = await context.PaymentAttempts.IgnoreQueryFilters()
            .Where(Attempt => Attempt.Id == attemptId && Attempt.EnterpriseId == enterpriseId && Attempt.Version == attempt.Version &&
                (Attempt.LeaseExpiresAt == null || Attempt.LeaseExpiresAt <= now) &&
                (Attempt.Status != PaymentCreationStatus.Requested ||
                    (Attempt.ExpiresAt > now && !Attempt.Order.IsDeleted && Attempt.Order.OrderStatusId == OrderStatus.Pending.Id &&
                        Attempt.Transaction.TransactionStatusId == TransactionStatus.Pending.Id &&
                        (Attempt.ProviderConnectionId == null || Attempt.ProviderConnection!.IsConnected))))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Attempt => Attempt.Status, PaymentCreationStatus.Processing)
                .SetProperty(Attempt => Attempt.LeaseToken, lease)
                .SetProperty(Attempt => Attempt.LeaseExpiresAt, now.AddMinutes(2))
                .SetProperty(Attempt => Attempt.Version, Attempt => Attempt.Version + 1)
                .SetProperty(Attempt => Attempt.UpdatedAt, now), ct);

        return changed == 1 ? new PaymentAttemptWork(attempt, lease, attempt.Status != PaymentCreationStatus.Requested) : null;
    }

    public async Task CompleteAsync(PaymentAttemptWork work, PaymentSession session, CancellationToken ct)
    {
        ValidateSession(session);
        var attempt = work.Attempt;
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        var changed = await context.PaymentAttempts.IgnoreQueryFilters()
            .Where(Attempt => Attempt.Id == attempt.Id && Attempt.EnterpriseId == attempt.EnterpriseId && Attempt.LeaseToken == work.LeaseToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Attempt => Attempt.Status, PaymentCreationStatus.Created)
                .SetProperty(Attempt => Attempt.PreferenceId, session.PreferenceId)
                .SetProperty(Attempt => Attempt.CheckoutUrl, session.CheckoutUrl)
                .SetProperty(Attempt => Attempt.Instructions, session.Instructions)
                .SetProperty(Attempt => Attempt.LeaseToken, (Guid?)null)
                .SetProperty(Attempt => Attempt.LeaseExpiresAt, (DateTime?)null)
                .SetProperty(Attempt => Attempt.Version, Attempt => Attempt.Version + 1)
                .SetProperty(Attempt => Attempt.UpdatedAt, time.GetUtcNow().UtcDateTime), ct);

        if (changed == 0)
            return;

        if (session.PaymentId is not null)
            await AddExternalPaymentAsync(attempt, session.PaymentId, ct);

        // Apenas espelha as instruções. Não confirma pagamento nem altera o estado comercial do pedido.
        var payload = new PaymentSessionPayload(session.CheckoutUrl, session.Instructions, session.ExpiresAt).Serialize();
        await context.Transactions.IgnoreQueryFilters()
            .Where(Transaction => Transaction.Id == attempt.TransactionId && Transaction.OrderId == attempt.OrderId &&
                Transaction.Order.EnterpriseId == attempt.EnterpriseId && Transaction.GatewayProvider == attempt.Provider &&
                (Transaction.GatewayReference == string.Empty || Transaction.TransactionStatusId == TransactionStatus.Pending.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Transaction => Transaction.GatewayReference, session.Reference)
                .SetProperty(Transaction => Transaction.GatewayPayload, payload), ct);

        using (scope.Begin(attempt.EnterpriseId))
            await context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
    }

    public async Task MarkUnknownAsync(PaymentAttemptWork work, DateTime nextReconciliationAt, CancellationToken ct)
    {
        await context.PaymentAttempts.IgnoreQueryFilters()
            .Where(Attempt => Attempt.Id == work.Attempt.Id && Attempt.EnterpriseId == work.Attempt.EnterpriseId && Attempt.LeaseToken == work.LeaseToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Attempt => Attempt.Status, PaymentCreationStatus.OutcomeUnknown)
                .SetProperty(Attempt => Attempt.LeaseToken, (Guid?)null)
                .SetProperty(Attempt => Attempt.LeaseExpiresAt, (DateTime?)null)
                .SetProperty(Attempt => Attempt.NextReconciliationAt, nextReconciliationAt)
                .SetProperty(Attempt => Attempt.Version, Attempt => Attempt.Version + 1)
                .SetProperty(Attempt => Attempt.UpdatedAt, time.GetUtcNow().UtcDateTime), ct);
    }

    public async Task<ExternalPaymentOwner> RegisterVerifiedPaymentAsync(VerifiedExternalPayment payment, CancellationToken ct)
    {
        var attempt = await context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Attempt => Attempt.EnterpriseId == payment.EnterpriseId && Attempt.Provider == payment.Provider &&
                Attempt.Environment == payment.Environment && Attempt.ExternalAccountId == payment.ExternalAccountId &&
                Attempt.ExternalReference == payment.ExternalReference, ct)
            ?? throw new SecurityFailure(404, "Payment.AttemptNotFound", "Tentativa não encontrada nesta conta e ambiente.");

        if (payment.Amount != attempt.Amount || payment.CurrencyCode != attempt.CurrencyCode)
            throw new SecurityFailure(409, "Payment.AmountMismatch", "Valor ou moeda não correspondem à tentativa.");

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        await AddExternalPaymentAsync(attempt, payment.PaymentId, ct);

        using (scope.Begin(attempt.EnterpriseId))
            await context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return new ExternalPaymentOwner(attempt.EnterpriseId, attempt.OrderId, attempt.Id);
    }

    private async Task AddExternalPaymentAsync(PaymentAttempt attempt, string paymentId, CancellationToken ct)
    {
        var external = PaymentExternalPayment.Create(attempt, paymentId);
        // Serializa apenas a associação desse ID externo; nenhuma chamada remota ocorre com o lock aberto.
        var lockKey = $"payment-id:{attempt.Provider}:{(int)attempt.Environment}:{attempt.ExternalAccountId}:{paymentId}";
        await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", ct);
        var existing = await context.PaymentExternalPayments.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Payment => Payment.Provider == attempt.Provider && Payment.Environment == attempt.Environment &&
                Payment.ExternalAccountId == attempt.ExternalAccountId && Payment.PaymentId == paymentId, ct);

        if (existing is not null && existing.PaymentAttemptId != attempt.Id)
            throw new SecurityFailure(409, "Payment.AlreadyLinked", "O pagamento já pertence a outra tentativa.");

        if (existing is null)
            context.PaymentExternalPayments.Add(external);
    }

    private static void ValidateSession(PaymentSession session)
    {
        if (string.IsNullOrWhiteSpace(session.Reference) || session.Reference.Length > 200 || session.PreferenceId?.Length > 200 ||
            session.PaymentId?.Length > 200 || session.CheckoutUrl?.Length > 2000 || session.Instructions?.Length > 1000)
            throw new SecurityFailure(409, "Payment.InvalidSession", "O provedor retornou uma sessão de pagamento inválida.");

        if (session.CheckoutUrl is not null &&
            (!Uri.TryCreate(session.CheckoutUrl, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps))
            throw new SecurityFailure(409, "Payment.InvalidCheckoutUrl", "O provedor retornou um endereço de pagamento inválido.");
    }
}
