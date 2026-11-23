using StoreCommerce.Application.Enums.Payments;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Enums.Payments;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Features.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoNotifications(StoreCommerceDbContext context, IntegrationWriteScope scope,
    MercadoPagoSignature signature, MercadoPagoGateway gateway, IPaymentAttemptStore attempts,
    IOptions<MercadoPagoOptions> options, TimeProvider time,
    ILogger<MercadoPagoNotifications> logger, IMediator mediator)
{
    public async Task ReceiveAsync(long connectionId, string paymentId, string requestId, string signatureHeader, CancellationToken ct)
    {
        var connection = await context.ProviderConnections.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Connection => Connection.Id == connectionId && Connection.Provider == IntegrationProviders.MercadoPago, ct)
            ?? throw new SecurityFailure(404, "MercadoPago.ConnectionNotFound", "Conexão não encontrada.");
        options.Value.RequireEnvironment(connection.Environment);
        var delivery = signature.Verify(paymentId, requestId, signatureHeader, connection.Environment);
        var notification = ProviderNotification.Create(connection, paymentId, delivery, time.GetUtcNow().UtcDateTime);
        context.Add(notification);

        try
        {
            using (scope.Begin(connection.EnterpriseId))
                await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // Repetição de uma entrega já aceita. Não repete os efeitos comerciais.
        }
        finally
        {
            context.Entry(notification).State = EntityState.Detached;
        }
    }

    public Task<List<MercadoPagoNotificationKey>> DueAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return context.Set<ProviderNotification>().IgnoreQueryFilters().AsNoTracking()
            .Where(Notification => Notification.Status != ProviderNotificationStatus.Completed &&
                Notification.Status != ProviderNotificationStatus.ReviewRequired && Notification.NextAttemptAt <= now &&
                (Notification.LeaseExpiresAt == null || Notification.LeaseExpiresAt <= now))
            .OrderBy(Notification => Notification.NextAttemptAt).ThenBy(Notification => Notification.Id).Take(50)
            .Select(Notification => new MercadoPagoNotificationKey(Notification.EnterpriseId, Notification.Id)).ToListAsync(ct);
    }

    public Task<int> ClearExpiredAuthorizationsAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return context.Set<ProviderAuthorization>().IgnoreQueryFilters()
            .Where(Authorization => Authorization.ExpiresAt <= now && Authorization.ProtectedVerifier != string.Empty)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Authorization => Authorization.ProtectedVerifier, string.Empty)
                .SetProperty(Authorization => Authorization.ConsumedAt, now), ct);
    }

    public async Task ProcessAsync(long enterpriseId, long id, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var lease = Guid.NewGuid();
        var claimed = await context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.Id == id && Notification.EnterpriseId == enterpriseId &&
                Notification.Status != ProviderNotificationStatus.Completed && Notification.Status != ProviderNotificationStatus.ReviewRequired &&
                Notification.NextAttemptAt <= now && (Notification.LeaseExpiresAt == null || Notification.LeaseExpiresAt <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Notification => Notification.Status, ProviderNotificationStatus.Processing)
                .SetProperty(Notification => Notification.LeaseToken, lease)
                .SetProperty(Notification => Notification.LeaseExpiresAt, now.AddMinutes(2))
                .SetProperty(Notification => Notification.Version, Notification => Notification.Version + 1), ct);

        if (claimed != 1)
            return;

        var notification = await context.Set<ProviderNotification>().IgnoreQueryFilters().AsNoTracking()
            .Include(Notification => Notification.Connection)
            .SingleAsync(Notification => Notification.Id == id && Notification.EnterpriseId == enterpriseId, ct);
        var status = ProviderNotificationStatus.Completed;
        var code = "Verified";

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            await ApplyPaymentAsync(notification.Connection, notification.PaymentId, timeout.Token);
        }
        catch (SecurityFailure failure) when (failure.Code is "MercadoPago.ScopeMismatch" or "MercadoPago.PaymentMismatch" or
            "Payment.AmountMismatch" or "Payment.ScopeMismatch" or "Payment.PartialRefundNeedsReview" or
            "MercadoPago.StatusNeedsReview" or "MercadoPago.EnvironmentMismatch")
        {
            status = ProviderNotificationStatus.ReviewRequired;
            code = "FinancialReviewRequired";
        }
        catch (Exception)
        {
            status = ProviderNotificationStatus.Pending;
            code = "ProviderOrPersistenceUnavailable";
            logger.LogWarning("Notificação Mercado Pago {NotificationId} pendente de consulta.", id);
        }

        using var persistence = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.Id == id && Notification.EnterpriseId == enterpriseId && Notification.LeaseToken == lease)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Notification => Notification.Status, status)
                .SetProperty(Notification => Notification.ResolutionCode, code)
                .SetProperty(Notification => Notification.LeaseToken, (Guid?)null)
                .SetProperty(Notification => Notification.LeaseExpiresAt, (DateTime?)null)
                .SetProperty(Notification => Notification.NextAttemptAt, time.GetUtcNow().UtcDateTime.AddMinutes(1))
                .SetProperty(Notification => Notification.Version, Notification => Notification.Version + 1), persistence.Token);
    }

    private async Task ApplyPaymentAsync(ProviderConnection connection, string paymentId, CancellationToken ct)
    {
        var snapshot = new PaymentProviderContext(connection.Provider, connection.Environment, connection.ExternalAccountId, connection.Id);
        var payment = await gateway.GetPaymentAsync(connection.EnterpriseId, snapshot, paymentId, ct);
        var attempt = await context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Attempt => Attempt.EnterpriseId == connection.EnterpriseId && Attempt.ProviderConnectionId == connection.Id &&
                Attempt.Provider == connection.Provider && Attempt.Environment == connection.Environment &&
                Attempt.ExternalAccountId == connection.ExternalAccountId && Attempt.ExternalReference == payment.ExternalReference, ct);

        if (attempt is null)
        {
            var drop = await context.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(Intent => Intent.EnterpriseId == connection.EnterpriseId &&
                    Intent.ProviderConnectionId == connection.Id && Intent.ExternalReference == payment.ExternalReference &&
                    Intent.ExternalAccountId == connection.ExternalAccountId && Intent.Environment == connection.Environment, ct);

            if (drop is not null)
                await mediator.SendRequiredAsync(new ReconcileDropSalePaymentCommand(drop.EnterpriseId, drop.OperationId), ct);

            return;
        }

        await context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.EnterpriseId == connection.EnterpriseId && Notification.ProviderConnectionId == connection.Id &&
                Notification.PaymentId == payment.Id && (Notification.PaymentAttemptId == null || Notification.PaymentAttemptId == attempt.Id))
            .ExecuteUpdateAsync(setters => setters.SetProperty(Notification => Notification.PaymentAttemptId, attempt.Id), ct);
        MercadoPagoGateway.ValidatePayment(payment, attempt.ExternalReference, attempt.Amount, attempt.CurrencyCode);
        var verified = new VerifiedExternalPayment(connection.EnterpriseId, connection.Provider, connection.Environment,
            connection.ExternalAccountId, payment.ExternalReference, payment.Id, payment.Amount, payment.CurrencyCode);
        var owner = await attempts.RegisterVerifiedPaymentAsync(verified, ct);
        var status = payment.Status switch
        {
            "pending" or "in_process" => VerifiedPaymentStatus.Pending,
            "authorized" => VerifiedPaymentStatus.Authorized,
            "approved" => VerifiedPaymentStatus.Approved,
            "rejected" => VerifiedPaymentStatus.Rejected,
            "cancelled" => VerifiedPaymentStatus.Cancelled,
            "refunded" => VerifiedPaymentStatus.Refunded,
            _ => throw new SecurityFailure(409, "MercadoPago.StatusNeedsReview", "O estado financeiro exige revisão.")
        };
        long? methodId = payment.MethodId == "pix" ? TransactionMethod.Pix.Id : payment.MethodType switch
        {
            "credit_card" => TransactionMethod.CreditCard.Id,
            "ticket" => TransactionMethod.Boleto.Id,
            "account_money" => TransactionMethod.MercadoPagoBalance.Id,
            _ => null
        };

        if (methodId is null && status is VerifiedPaymentStatus.Approved or VerifiedPaymentStatus.Authorized or VerifiedPaymentStatus.Refunded)
            throw new SecurityFailure(409, "MercadoPago.StatusNeedsReview", "Meio de pagamento ainda não suportado pelo domínio.");

        await mediator.Send(new ApplyVerifiedPaymentCommand(new VerifiedPaymentUpdate(owner, verified, connection.Id, status,
            payment.RefundedAmount, payment.UpdatedAt, methodId)), ct);
        await context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.EnterpriseId == connection.EnterpriseId && Notification.ProviderConnectionId == connection.Id &&
                Notification.PaymentId == payment.Id && Notification.Status == ProviderNotificationStatus.ReviewRequired)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Notification => Notification.Status, ProviderNotificationStatus.Completed)
                .SetProperty(Notification => Notification.ResolutionCode, "Reconciled")
                .SetProperty(Notification => Notification.Version, Notification => Notification.Version + 1), ct);
    }

    public Task<List<MercadoPagoNotificationKey>> DuePaymentsAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .Where(Attempt => Attempt.Provider == IntegrationProviders.MercadoPago && Attempt.Status == PaymentCreationStatus.Created &&
                Attempt.NextStatusCheckAt <= now)
            .OrderBy(Attempt => Attempt.NextStatusCheckAt).ThenBy(Attempt => Attempt.Id).Take(50)
            .Select(Attempt => new MercadoPagoNotificationKey(Attempt.EnterpriseId, Attempt.Id)).ToListAsync(ct);
    }

    public async Task ReconcilePaymentAsync(long enterpriseId, long attemptId, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var claimed = await context.PaymentAttempts.IgnoreQueryFilters()
            .Where(Attempt => Attempt.EnterpriseId == enterpriseId && Attempt.Id == attemptId &&
                Attempt.Status == PaymentCreationStatus.Created && Attempt.NextStatusCheckAt <= now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(Attempt => Attempt.NextStatusCheckAt, now.AddMinutes(5)), ct);

        if (claimed != 1)
            return;

        var attempt = await context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .Include(Attempt => Attempt.ProviderConnection)
            .SingleAsync(Attempt => Attempt.Id == attemptId && Attempt.EnterpriseId == enterpriseId, ct);
        var connection = attempt.ProviderConnection ?? throw new InvalidOperationException("Pagamento sem conexão original.");
        var snapshot = new PaymentProviderContext(attempt.Provider, attempt.Environment, attempt.ExternalAccountId, connection.Id);
        var payments = await gateway.SearchPaymentsAsync(enterpriseId, snapshot, attempt.ExternalReference, ct);

        foreach (var payment in payments)
        {
            // A busca pode ter índice defasado. A aplicação sempre consulta o ID antes de decidir o estado financeiro.
            context.ChangeTracker.Clear();

            try
            {
                await ApplyPaymentAsync(connection, payment.Id, ct);
            }
            catch (SecurityFailure failure) when (failure.Code is "MercadoPago.ScopeMismatch" or "MercadoPago.PaymentMismatch" or
                "Payment.AmountMismatch" or "Payment.ScopeMismatch" or "Payment.PartialRefundNeedsReview" or
                "MercadoPago.StatusNeedsReview" or "MercadoPago.EnvironmentMismatch")
            {
                context.ChangeTracker.Clear();
                var delivery = MercadoPagoAuthorizationService.Hash("reconciliation:" + payment.Id + ":" + payment.UpdatedAt.Ticks);
                var notification = ProviderNotification.Create(connection, payment.Id, delivery, now, attempt.Id);
                context.Add(notification);

                try
                {
                    using (scope.Begin(enterpriseId))
                        await context.SaveChangesAsync(ct);
                }
                catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
                {
                    context.Entry(notification).State = EntityState.Detached;
                }

                await context.Set<ProviderNotification>().IgnoreQueryFilters()
                    .Where(Notification => Notification.EnterpriseId == enterpriseId && Notification.ProviderConnectionId == connection.Id &&
                        Notification.DeliveryHash == delivery)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(Notification => Notification.Status, ProviderNotificationStatus.ReviewRequired)
                        .SetProperty(Notification => Notification.ResolutionCode, "FinancialReviewRequired"), ct);
            }
        }
    }
}
