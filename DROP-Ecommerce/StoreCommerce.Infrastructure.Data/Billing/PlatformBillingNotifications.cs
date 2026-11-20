using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Billing;

public sealed class PlatformBillingNotifications(StoreCommerceDbContext db, PlatformBillingSignature signature,
    PlatformBillingWriteScope writes, IIdentityOperationScope identity, IPlatformSubscriptionProvider recurring,
    IPlatformBillingProvider payments, PlatformBillingService billing, IOptions<PlatformBillingOptions> options, TimeProvider time)
{
    public async Task ReceiveAsync(string topic, string id, string requestId, string signatureHeader, CancellationToken ct)
    {
        if (topic is not ("payment" or "subscription_preapproval" or "subscription_authorized_payment"))
            throw new SecurityFailure(400, "Billing.InvalidTopic", "Tópico de cobrança não suportado.");

        var delivery = signature.Verify(id, requestId, signatureHeader);
        var notification = PlatformBillingNotification.Receive(delivery, topic, id, options.Value.AccountId,
            options.Value.Environment, time.GetUtcNow().UtcDateTime);
        using var operation = identity.Begin();
        using var financial = writes.Begin();
        db.Add(notification);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // A mesma entrega assinada já está durável. A concessão também é idempotente.
        }
        finally
        {
            db.Entry(notification).State = EntityState.Detached;
        }
    }

    public Task<List<long>> DueAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return db.Set<PlatformBillingNotification>().AsNoTracking().Where(Notification =>
            Notification.CompletedAt == null && Notification.NextCheckAt <= now)
            .OrderBy(Notification => Notification.NextCheckAt).ThenBy(Notification => Notification.Id)
            .Take(50).Select(Notification => Notification.Id).ToListAsync(ct);
    }

    public async Task ProcessAsync(long id, CancellationToken ct)
    {
        var notification = await db.Set<PlatformBillingNotification>().AsNoTracking().SingleAsync(Notification => Notification.Id == id, ct);

        if (notification.CompletedAt.HasValue)
            return;

        if (notification.AccountId != options.Value.AccountId || notification.Environment != options.Value.Environment)
            throw new SecurityFailure(409, "Billing.AccountChanged", "A notificação pertence à configuração financeira anterior.");

        var subscriptions = db.Set<PlatformSubscription>().IgnoreQueryFilters().AsNoTracking()
            .Where(Subscription => Subscription.AccountId == notification.AccountId && Subscription.Environment == notification.Environment);
        PlatformSubscription? subscription;

        if (notification.Topic == "subscription_preapproval")
        {
            var remote = await recurring.GetAsync(notification.ResourceId, ct);
            subscription = await subscriptions.SingleOrDefaultAsync(Subscription => Subscription.Reference == remote.Reference &&
                (Subscription.RemoteId == null || Subscription.RemoteId == remote.Id), ct);
        }
        else if (notification.Topic == "subscription_authorized_payment")
        {
            var invoice = await recurring.InvoiceAsync(notification.ResourceId, ct);
            subscription = await subscriptions.SingleOrDefaultAsync(Subscription => Subscription.RemoteId == invoice.SubscriptionId, ct);
        }
        else
        {
            var payment = await payments.GetPaymentAsync(notification.ResourceId, ct);
            var invoiceSubscription = await db.Set<PlatformBillingInvoice>().IgnoreQueryFilters().Where(Invoice =>
                Invoice.Reference == payment.ExternalReference || Invoice.PaymentId == payment.Id)
                .Select(Invoice => (long?)Invoice.SubscriptionId).SingleOrDefaultAsync(ct);
            subscription = await subscriptions.SingleOrDefaultAsync(Subscription => Subscription.Id == invoiceSubscription ||
                Subscription.Reference == payment.ExternalReference, ct);
        }

        if (subscription is not null)
            await billing.ReconcileAsync(subscription.EnterpriseId, subscription.Id, ct);

        db.ChangeTracker.Clear();
        using var operation = identity.Begin();
        using var financial = writes.Begin();
        var stored = await db.Set<PlatformBillingNotification>().SingleAsync(Notification => Notification.Id == id, ct);
        stored.Complete(time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync(ct);
    }

    public async Task RetryAsync(long id, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        using var operation = identity.Begin();
        using var financial = writes.Begin();
        var stored = await db.Set<PlatformBillingNotification>().SingleAsync(Notification => Notification.Id == id, ct);
        stored.Retry(time.GetUtcNow().UtcDateTime.AddMinutes(1));
        await db.SaveChangesAsync(ct);
    }
}
