using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Enums.Payments;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class ProviderNotificationStore(StoreCommerceDbContext context, IntegrationWriteScope scope) : IProviderNotificationStore
{
    public Task<List<ProviderNotificationSummary>> ListPendingAsync(long enterpriseId, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters().AsNoTracking()
            .Where(Notification => Notification.EnterpriseId == enterpriseId && Notification.Status != ProviderNotificationStatus.Completed)
            .OrderByDescending(Notification => Notification.Id).Take(100)
            .Select(Notification => new ProviderNotificationSummary(Notification.Id, Notification.ProviderConnectionId, Notification.PaymentAttemptId,
                Notification.PaymentId, Notification.Status, Notification.ResolutionCode, Notification.CreatedAt)).ToListAsync(ct);
    }

    public Task<ProviderConnection?> FindConnectionAsync(long connectionId, CancellationToken ct)
    {
        return context.ProviderConnections.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Connection => Connection.Id == connectionId && Connection.Provider == IntegrationProviders.MercadoPago, ct);
    }

    public Task<int> ClaimNotificationAsync(long enterpriseId, long id, DateTime now, Guid lease, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.Id == id && Notification.EnterpriseId == enterpriseId &&
                Notification.Status != ProviderNotificationStatus.Completed && Notification.Status != ProviderNotificationStatus.ReviewRequired &&
                Notification.NextAttemptAt <= now && (Notification.LeaseExpiresAt == null || Notification.LeaseExpiresAt <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Notification => Notification.Status, ProviderNotificationStatus.Processing)
                .SetProperty(Notification => Notification.LeaseToken, lease)
                .SetProperty(Notification => Notification.LeaseExpiresAt, now.AddMinutes(2))
                .SetProperty(Notification => Notification.Version, Notification => Notification.Version + 1), ct);
    }

    public Task<ProviderNotification> GetNotificationAsync(long enterpriseId, long id, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters().AsNoTracking()
            .Include(Notification => Notification.Connection)
            .SingleAsync(Notification => Notification.Id == id && Notification.EnterpriseId == enterpriseId, ct);
    }

    public Task<int> CompleteNotificationLeaseAsync(long enterpriseId, long id, Guid lease, ProviderNotificationStatus status, string code, DateTime nextAttemptAt, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.Id == id && Notification.EnterpriseId == enterpriseId && Notification.LeaseToken == lease)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Notification => Notification.Status, status)
                .SetProperty(Notification => Notification.ResolutionCode, code)
                .SetProperty(Notification => Notification.LeaseToken, (Guid?)null)
                .SetProperty(Notification => Notification.LeaseExpiresAt, (DateTime?)null)
                .SetProperty(Notification => Notification.NextAttemptAt, nextAttemptAt)
                .SetProperty(Notification => Notification.Version, Notification => Notification.Version + 1), ct);
    }

    public Task<PaymentAttempt?> FindPaymentAttemptAsync(ProviderConnection connection, string externalReference, CancellationToken ct)
    {
        return context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Attempt => Attempt.EnterpriseId == connection.EnterpriseId && Attempt.ProviderConnectionId == connection.Id &&
                Attempt.Provider == connection.Provider && Attempt.Environment == connection.Environment &&
                Attempt.ExternalAccountId == connection.ExternalAccountId && Attempt.ExternalReference == externalReference, ct);
    }

    public Task<DropSalePaymentIntent?> FindDropPaymentIntentAsync(ProviderConnection connection, string externalReference, CancellationToken ct)
    {
        return context.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(Intent => Intent.EnterpriseId == connection.EnterpriseId &&
                    Intent.ProviderConnectionId == connection.Id && Intent.ExternalReference == externalReference &&
                    Intent.ExternalAccountId == connection.ExternalAccountId && Intent.Environment == connection.Environment, ct);
    }

    public Task<int> LinkPaymentAttemptAsync(ProviderConnection connection, string paymentId, long attemptId, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.EnterpriseId == connection.EnterpriseId && Notification.ProviderConnectionId == connection.Id &&
                Notification.PaymentId == paymentId && (Notification.PaymentAttemptId == null || Notification.PaymentAttemptId == attemptId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(Notification => Notification.PaymentAttemptId, attemptId), ct);
    }

    public Task<int> CompleteReviewedNotificationsAsync(ProviderConnection connection, string paymentId, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters()
            .Where(Notification => Notification.EnterpriseId == connection.EnterpriseId && Notification.ProviderConnectionId == connection.Id &&
                Notification.PaymentId == paymentId && Notification.Status == ProviderNotificationStatus.ReviewRequired)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Notification => Notification.Status, ProviderNotificationStatus.Completed)
                .SetProperty(Notification => Notification.ResolutionCode, "Reconciled")
                .SetProperty(Notification => Notification.Version, Notification => Notification.Version + 1), ct);
    }

    public Task<int> ClaimPaymentCheckAsync(long enterpriseId, long attemptId, DateTime now, CancellationToken ct)
    {
        return context.PaymentAttempts.IgnoreQueryFilters()
            .Where(Attempt => Attempt.EnterpriseId == enterpriseId && Attempt.Id == attemptId &&
                Attempt.Status == PaymentCreationStatus.Created && Attempt.NextStatusCheckAt <= now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(Attempt => Attempt.NextStatusCheckAt, now.AddMinutes(5)), ct);
    }

    public Task<PaymentAttempt> GetPaymentAttemptAsync(long enterpriseId, long attemptId, CancellationToken ct)
    {
        return context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .Include(Attempt => Attempt.ProviderConnection)
            .SingleAsync(Attempt => Attempt.Id == attemptId && Attempt.EnterpriseId == enterpriseId, ct);
    }

    public Task<int> MarkDeliveryForReviewAsync(long enterpriseId, long connectionId, string delivery, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters()
                    .Where(Notification => Notification.EnterpriseId == enterpriseId && Notification.ProviderConnectionId == connectionId &&
                        Notification.DeliveryHash == delivery)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(Notification => Notification.Status, ProviderNotificationStatus.ReviewRequired)
                        .SetProperty(Notification => Notification.ResolutionCode, "FinancialReviewRequired"), ct);
    }

    public Task<List<MercadoPagoNotificationKey>> ReadDueAsync(DateTime now, CancellationToken ct)
    {
        return context.Set<ProviderNotification>().IgnoreQueryFilters().AsNoTracking()
            .Where(Notification => Notification.Status != ProviderNotificationStatus.Completed &&
                Notification.Status != ProviderNotificationStatus.ReviewRequired && Notification.NextAttemptAt <= now &&
                (Notification.LeaseExpiresAt == null || Notification.LeaseExpiresAt <= now))
            .OrderBy(Notification => Notification.NextAttemptAt).ThenBy(Notification => Notification.Id).Take(50)
            .Select(Notification => new MercadoPagoNotificationKey(Notification.EnterpriseId, Notification.Id)).ToListAsync(ct);
    }

    public Task<List<MercadoPagoNotificationKey>> ReadDuePaymentsAsync(DateTime now, CancellationToken ct)
    {
        return context.PaymentAttempts.IgnoreQueryFilters().AsNoTracking()
            .Where(Attempt => Attempt.Provider == IntegrationProviders.MercadoPago && Attempt.Status == PaymentCreationStatus.Created &&
                Attempt.NextStatusCheckAt <= now)
            .OrderBy(Attempt => Attempt.NextStatusCheckAt).ThenBy(Attempt => Attempt.Id).Take(50)
            .Select(Attempt => new MercadoPagoNotificationKey(Attempt.EnterpriseId, Attempt.Id)).ToListAsync(ct);
    }

    public Task<int> ClearExpiredAuthorizationsAsync(DateTime now, CancellationToken ct)
    {
        return context.Set<ProviderAuthorization>().IgnoreQueryFilters()
            .Where(Authorization => Authorization.ExpiresAt <= now && Authorization.ProtectedVerifier != string.Empty)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Authorization => Authorization.ProtectedVerifier, string.Empty)
                .SetProperty(Authorization => Authorization.ConsumedAt, now), ct);
    }

    public void ClearTracking()
    {
        context.ChangeTracker.Clear();
    }

    public async Task InsertNotificationAsync(ProviderNotification notification, bool detachAfterInsert, CancellationToken ct)
    {
        context.Add(notification);

        try
        {
            using (scope.Begin(notification.EnterpriseId))
                await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            context.Entry(notification).State = EntityState.Detached;
        }
        finally
        {
            if (detachAfterInsert)
                context.Entry(notification).State = EntityState.Detached;
        }
    }
}
