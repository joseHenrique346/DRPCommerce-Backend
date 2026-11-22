using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProcessPlatformBillingNotificationCommandHandler(
    IPlatformBillingNotificationRepository persistence,
    PlatformBillingWriteScope writes,
    IIdentityOperationScope identity,
    IPlatformSubscriptionProvider recurring,
    IPlatformBillingProvider payments,
    IOptions<PlatformBillingOptions> options,
    TimeProvider clock,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ProcessPlatformBillingNotificationCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(ProcessPlatformBillingNotificationCommand command, CancellationToken ct)
    {
        long id = command.NotificationId;

        var notification = await persistence.Notifications.GetRequiredAsync(Notification => Notification.Id == id, ct, tracking: false);

        if (notification.CompletedAt.HasValue)
            return true;

        if (notification.AccountId != options.Value.AccountId || notification.Environment != options.Value.Environment)
            throw new SecurityFailure(409, "Billing.AccountChanged", "A notificação pertence à configuração financeira anterior.");

        PlatformSubscription? subscription;

        if (notification.Topic == "subscription_preapproval")
        {
            var remote = await recurring.GetAsync(notification.ResourceId, ct);
            subscription = await persistence.Subscriptions.FindAsync(Subscription => Subscription.AccountId == notification.AccountId &&
                Subscription.Environment == notification.Environment && (Subscription.Reference == remote.Reference &&
                (Subscription.RemoteId == null || Subscription.RemoteId == remote.Id)), ct, ignoreQueryFilters: true, tracking: false);
        }
        else if (notification.Topic == "subscription_authorized_payment")
        {
            var invoice = await recurring.InvoiceAsync(notification.ResourceId, ct);
            subscription = await persistence.Subscriptions.FindAsync(Subscription => Subscription.AccountId == notification.AccountId &&
                Subscription.Environment == notification.Environment && (Subscription.RemoteId == invoice.SubscriptionId), ct, ignoreQueryFilters: true, tracking: false);
        }
        else
        {
            var payment = await payments.GetPaymentAsync(notification.ResourceId, ct);
            var invoiceSubscription = await persistence.Invoices.FindValueAsync(Invoice =>
                Invoice.Reference == payment.ExternalReference || Invoice.PaymentId == payment.Id,
                Invoice => (long?)Invoice.SubscriptionId, ct, ignoreQueryFilters: true);
            subscription = await persistence.Subscriptions.FindAsync(Subscription => Subscription.AccountId == notification.AccountId &&
                Subscription.Environment == notification.Environment && (Subscription.Id == invoiceSubscription ||
                Subscription.Reference == payment.ExternalReference), ct, ignoreQueryFilters: true, tracking: false);
        }

        if (subscription is not null)
            await mediator.SendRequiredAsync(new ReconcilePlatformSubscriptionCommand(subscription.EnterpriseId, subscription.Id), ct);

        persistence.ClearTracking();
        using var operation = identity.Begin();
        using var financial = writes.Begin();
        var stored = await persistence.Notifications.GetRequiredAsync(Notification => Notification.Id == id, ct);
        stored.Complete(clock.GetUtcNow().UtcDateTime);
        await persistence.SaveChangesAsync(ct);

        return true;
    }
}
