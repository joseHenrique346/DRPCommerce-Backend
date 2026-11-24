using StoreCommerce.Application.Interfaces.Billing;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReconcilePlatformSubscriptionCommandHandler(
    IPlatformBillingRepository persistence,
    IIdentityOperationScope identity,
    PlatformBillingWriteScope writes,
    IPlatformSubscriptionProvider recurring,
    IPlatformBillingProvider payments,
    IOptions<PlatformBillingOptions> options,
    TimeProvider clock,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ReconcilePlatformSubscriptionCommand, bool>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;
    private IEntityRepository<PlatformSubscription> Subscriptions => persistence.Subscriptions;
    private IEntityRepository<PlatformBillingInvoice> Invoices => persistence.Invoices;

    protected override async Task<bool> ExecuteAsync(ReconcilePlatformSubscriptionCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long id = command.SubscriptionId;

        options.Value.RequireAccount();

        try
        {
            using var operation = identity.Begin();
            using var billing = writes.Begin();
            await EnsureSubscriptionAsync(tenant, id, ct);
            await EnsureUpgradeCheckoutAsync(tenant, id, ct);
            persistence.ClearTracking();
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);
            await LockAsync(tenant, ct);
            var subscription = await SubscriptionAsync(tenant, id, ct);
            RequireSnapshot(subscription);

            if (subscription.RemoteId is null || subscription.ClosedAt.HasValue)
                return true;

            var remote = await recurring.GetAsync(subscription.RemoteId, ct);
            ValidateRemote(subscription, remote);
            subscription.ObserveRemote(remote.Id, remote.CheckoutUrl, remote.Status);
            await ObserveRecurringInvoicesAsync(subscription, ct);
            await ObserveUpgradeAsync(subscription, ct);
            await SynchronizeChangeAsync(subscription, remote, ct);
            await ApplyCurrentRightsAsync(subscription, ct);

            if (subscription.ReviewCode is null)
            {
                foreach (var invoice in persistence.TrackedInvoices
                    .Where(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.Status == "approved" &&
                        Invoice.ReviewCode == null && Invoice.OfferId == subscription.OfferId &&
                        Invoice.PeriodEnd == subscription.PaidUntil))
                    invoice.Applied(Now);
            }

            subscription.CheckAfter(Now.AddMinutes(1));
            await unitOfWork.CommitAsync(ct);
            await tx.CommitAsync(ct);
        }
        finally
        {
            // Uma indisponibilidade do provedor nunca prolonga os sete dias de carência.
            persistence.ClearTracking();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await mediator.SendRequiredAsync(new EnforcePlatformSubscriptionExpiryCommand(tenant, id), timeout.Token);
        }

        return true;
    }

    private async Task EnsureSubscriptionAsync(long tenant, long id, CancellationToken ct)
    {
        PlatformSubscription subscription;
        EnterpriseContractChange offer;
        string? email = null;
        bool create;

        await using (var tx = await unitOfWork.BeginTransactionAsync(ct))
        {
            await LockAsync(tenant, ct);
            subscription = await SubscriptionAsync(tenant, id, ct);
            RequireSnapshot(subscription);

            if (subscription.RemoteId is not null || subscription.ClosedAt.HasValue)
                return;

            offer = await OfferAsync(subscription.OfferId, tenant, ct);
            create = !subscription.SubmittedAt.HasValue;

            if (create)
            {
                var payer = await data.Users.FindAsync(User => User.Id == offer.RequestedByUserId && User.IsActive, ct,
                    ignoreQueryFilters: true, tracking: false);
                email = payer?.Email.Value;

                if (string.IsNullOrWhiteSpace(email))
                    throw Conflict("Billing.PayerRequired", "O responsável pela contratação precisa estar ativo.");

                subscription.Submit(Now);
                await unitOfWork.CommitAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        var remote = create ? await recurring.CreateAsync(subscription.Reference, email!, Monthly(offer), ct) :
            await recurring.RecoverAsync(subscription.Reference, ct);

        if (remote is null)
            return;

        persistence.ClearTracking();
        await using var persist = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        subscription = await SubscriptionAsync(tenant, id, ct);
        ValidateRemote(subscription, remote);

        if (remote.MonthlyAmount != Monthly(offer))
            throw Conflict("Billing.SubscriptionMismatch", "O valor da assinatura não corresponde à oferta.");

        subscription.ObserveRemote(remote.Id, remote.CheckoutUrl, remote.Status);
        await unitOfWork.CommitAsync(ct);
        await persist.CommitAsync(ct);
    }

    private async Task EnsureUpgradeCheckoutAsync(long tenant, long id, CancellationToken ct)
    {
        persistence.ClearTracking();
        PlatformBillingInvoice? invoice;
        PlatformSubscription subscription;
        bool create;

        await using (var tx = await unitOfWork.BeginTransactionAsync(ct))
        {
            await LockAsync(tenant, ct);
            subscription = await SubscriptionAsync(tenant, id, ct);

            if (!subscription.IsUpgrade || subscription.ReviewCode is not null)
                return;

            invoice = await Invoices.GetRequiredAsync(Invoice => Invoice.SubscriptionId == id &&
                Invoice.OfferId == subscription.PendingOfferId && Invoice.IsUpgrade, ct, ignoreQueryFilters: true);

            if (invoice.PreferenceId is not null || invoice.ReviewCode is not null)
                return;

            create = !invoice.SubmittedAt.HasValue;

            if (create && Now >= invoice.CheckoutExpiresAt)
            {
                invoice.ExpireCheckout();
                var current = await OfferAsync(subscription.OfferId, tenant, ct);
                subscription.AbandonUpgrade(current.RetainedMembershipIds);
                await unitOfWork.CommitAsync(ct);
                await tx.CommitAsync(ct);

                return;
            }

            if (create)
            {
                invoice.Submit(Now);
                await unitOfWork.CommitAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        var charge = Charge(subscription, invoice);
        var checkout = create ? await payments.CreateAsync(charge, ct) : await payments.RecoverAsync(charge, ct);

        if (checkout is null)
            return;

        persistence.ClearTracking();
        await using var persist = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        invoice = await Invoices.GetRequiredAsync(Invoice => Invoice.Id == invoice.Id && Invoice.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
        invoice.Checkout(checkout.PreferenceId, checkout.CheckoutUrl);
        await unitOfWork.CommitAsync(ct);
        await persist.CommitAsync(ct);
    }

    private async Task ObserveRecurringInvoicesAsync(PlatformSubscription subscription, CancellationToken ct)
    {
        var remoteInvoices = await recurring.InvoicesAsync(subscription.RemoteId!, ct);

        foreach (var remote in remoteInvoices.OrderBy(Invoice => Invoice.DebitDate))
        {
            var origin = "recurring:" + remote.Id;
            var invoice = await Invoices.FindAsync(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.Origin == origin, ct, ignoreQueryFilters: true);

            if (invoice is null)
            {
                var offerId = subscription.PendingOfferId.HasValue && !subscription.IsUpgrade &&
                    remote.DebitDate >= subscription.ChangeEffectiveAt ? subscription.PendingOfferId.Value : subscription.OfferId;
                var offer = await OfferAsync(offerId, subscription.EnterpriseId, ct);

                if (remote.SubscriptionId != subscription.RemoteId || remote.Amount != Monthly(offer) || remote.Currency != offer.Currency)
                {
                    subscription.Review("Billing.InvoiceMismatch");

                    continue;
                }

                invoice = PlatformBillingInvoice.Create(subscription, offer, origin, false, remote.Amount,
                    remote.DebitDate, PlatformBillingPolicy.PeriodEnd(remote.DebitDate, subscription.BillingAnchor), remote.DebitDate);
                Invoices.Add(invoice);
            }

            var paymentId = remote.PaymentId ?? invoice.PaymentId;

            if (paymentId is null)
                continue;

            var payment = await payments.GetPaymentAsync(paymentId, ct);
            await ObservePaymentAsync(subscription, invoice, payment, false, ct);

            if (invoice.Status == "approved" && invoice.ReviewCode is null && invoice.PeriodStart <= Now &&
                (!subscription.PaidUntil.HasValue || invoice.PeriodEnd > subscription.PaidUntil.Value))
            {
                subscription.Renew(invoice.PeriodStart, invoice.PeriodEnd);
                subscription.ApplyOffer(invoice.OfferId);
            }
        }
    }

    private async Task ObserveUpgradeAsync(PlatformSubscription subscription, CancellationToken ct)
    {
        var invoices = await Invoices.ListAsync(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.IsUpgrade, ct, ignoreQueryFilters: true);

        foreach (var invoice in invoices)
            await ObserveUpgradeInvoiceAsync(subscription, invoice, ct);
    }

    private async Task SynchronizeChangeAsync(PlatformSubscription subscription, PlatformRecurringSubscription remote, CancellationToken ct)
    {
        if (!subscription.PendingOfferId.HasValue)
            return;

        var offer = await OfferAsync(subscription.PendingOfferId.Value, subscription.EnterpriseId, ct);

        if (subscription.ReviewCode is not null && Monthly(offer) != 0)
            return;

        PlatformBillingInvoice? upgrade = null;

        if (subscription.IsUpgrade)
        {
            upgrade = await Invoices.GetRequiredAsync(Invoice => Invoice.SubscriptionId == subscription.Id &&
                Invoice.OfferId == offer.Id && Invoice.IsUpgrade, ct, ignoreQueryFilters: true);

            if (upgrade.Status != "approved" || upgrade.ReviewCode is not null)
                return;
        }

        var cancel = Monthly(offer) == 0;

        if (cancel && remote.Status is not ("cancelled" or "canceled") || !cancel && remote.MonthlyAmount != Monthly(offer))
        {
            remote = await recurring.UpdateAsync(subscription.RemoteId!, Monthly(offer), cancel, ct);
            ValidateRemote(subscription, remote);
        }

        subscription.ObserveRemote(remote.Id, remote.CheckoutUrl, remote.Status);
        subscription.SynchronizeChange();

        if (subscription.IsUpgrade || Now >= subscription.ChangeEffectiveAt)
        {
            subscription.ApplyOffer(offer.Id);

            if (cancel)
                subscription.Close(Now);
        }
    }

    private async Task ApplyCurrentRightsAsync(PlatformSubscription subscription, CancellationToken ct)
    {
        var expired = subscription.PaidUntil.HasValue && Now >= subscription.PaidUntil.Value.AddDays(PlatformBillingPolicy.GraceDays);
        var offer = await OfferAsync(subscription.OfferId, subscription.EnterpriseId, ct);

        if (subscription.ClosedAt.HasValue && Monthly(offer) == 0)
        {
            await mediator.SendRequiredAsync(new ApplyPlatformSubscriptionRightsCommand(subscription.EnterpriseId, subscription.Id, null), ct);

            return;
        }

        if (!subscription.PaidUntil.HasValue && subscription.RemoteStatus is "canceled" or "cancelled")
        {
            subscription.Close(Now);

            return;
        }

        if (!subscription.PaidUntil.HasValue || subscription.ReviewCode is not null && !expired)
            return;

        await mediator.SendRequiredAsync(new ApplyPlatformSubscriptionRightsCommand(subscription.EnterpriseId, subscription.Id, (expired ? null : offer)?.Id), ct);
    }

    private Task<PlatformSubscription> SubscriptionAsync(long tenant, long id, CancellationToken ct)
    {
        return Subscriptions.GetRequiredAsync(Subscription => Subscription.Id == id && Subscription.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return data.Enterprises.LockAsync(tenant, ct);
    }

    private void RequireSnapshot(PlatformSubscription subscription)
    {
        if (subscription.AccountId != options.Value.AccountId || subscription.Environment != options.Value.Environment)
            throw Conflict("Billing.AccountChanged", "Restaure o recebedor e o ambiente originais da assinatura.");
    }

    private static void ValidateRemote(PlatformSubscription subscription, PlatformRecurringSubscription remote)
    {
        if (remote.AccountId != subscription.AccountId || remote.Reference != subscription.Reference || remote.Currency != "BRL" ||
            subscription.RemoteId is not null && subscription.RemoteId != remote.Id)
            throw Conflict("Billing.SubscriptionMismatch", "A assinatura externa não corresponde à contratação persistida.");
    }

    private static SecurityFailure Conflict(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }

    private async Task<EnterpriseContractChange> OfferAsync(long id, long tenant, CancellationToken ct)
    {
        return await persistence.Offers.FindAsync(Offer => Offer.Id == id && Offer.EnterpriseId == tenant, ct, ignoreQueryFilters: true)
            ?? throw new SecurityFailure(404, "Billing.OfferNotFound", "Escolha não encontrada.");
    }

    private static decimal Monthly(EnterpriseContractChange offer)
    {
        return offer.PlanMonthlyPrice + offer.AddOnMonthlyPrice;
    }

    private async Task ObserveUpgradeInvoiceAsync(PlatformSubscription subscription, PlatformBillingInvoice invoice, CancellationToken ct)
    {
        var observed = await payments.SearchPaymentsAsync(invoice.Reference, ct);

        if (observed.Count(Payment => Payment.Status == "approved" && Payment.RefundedAmount == 0) > 1)
        {
            invoice.Review("Billing.DuplicatePayment");
            subscription.Review("Billing.DuplicatePayment");

            return;
        }

        foreach (var payment in observed.OrderBy(Payment => Payment.UpdatedAt))
            await ObservePaymentAsync(subscription, invoice, payment, true, ct);

        if (invoice.Status == "approved" && !invoice.AppliedAt.HasValue &&
            (Now >= invoice.PeriodEnd || subscription.PendingOfferId != invoice.OfferId))
        {
            invoice.Review("Billing.LateUpgradePayment");
            subscription.Review("Billing.LateUpgradePayment");
        }

        if (invoice.Status != "approved" && !invoice.AppliedAt.HasValue && Now >= invoice.CheckoutExpiresAt &&
            (Now >= invoice.PeriodEnd || observed.All(Payment => Payment.Status is "rejected" or "cancelled" or "canceled")))
        {
            invoice.ExpireCheckout();

            if (subscription.IsUpgrade && subscription.PendingOfferId == invoice.OfferId)
            {
                var current = await OfferAsync(subscription.OfferId, subscription.EnterpriseId, ct);
                subscription.AbandonUpgrade(current.RetainedMembershipIds);
            }
        }
    }

    private async Task ObservePaymentAsync(PlatformSubscription subscription, PlatformBillingInvoice invoice,
        PlatformBillingPayment payment, bool checkReference, CancellationToken ct)
    {
        if (payment.AccountId != subscription.AccountId || payment.Amount != invoice.Amount || payment.Currency != invoice.Currency ||
            checkReference && payment.ExternalReference != invoice.Reference ||
            await Invoices.ExistsAsync(Invoice => Invoice.PaymentId == payment.Id && Invoice.Id != invoice.Id, ct, ignoreQueryFilters: true))
        {
            invoice.Review("Billing.PaymentMismatch");
            subscription.Review("Billing.PaymentMismatch");

            return;
        }

        try
        {
            invoice.Observe(payment.Id, payment.RefundedAmount == 0 ? payment.Status : "refunded", payment.UpdatedAt);
        }
        catch (InvalidOperationException)
        {
            invoice.Review("Billing.DuplicatePayment");
            subscription.Review("Billing.DuplicatePayment");

            return;
        }

        if (!invoice.IsUpgrade && invoice.Status == "approved" && persistence.TrackedInvoices.Any(Invoice =>
            !ReferenceEquals(Invoice, invoice) && Invoice.SubscriptionId == invoice.SubscriptionId &&
            !Invoice.IsUpgrade && Invoice.Status == "approved" && Invoice.PeriodStart == invoice.PeriodStart))
        {
            invoice.Review("Billing.DuplicatePayment");
            subscription.Review("Billing.DuplicatePayment");
        }

        if (payment.RefundedAmount > 0 || payment.Status is "refunded" or "charged_back")
        {
            invoice.Review("Billing.FinancialReversal");
            subscription.Review("Billing.FinancialReversal");
        }
    }

    private static PlatformBillingCharge Charge(PlatformSubscription subscription, PlatformBillingInvoice invoice)
    {
        return new PlatformBillingCharge(invoice.Reference, subscription.AccountId, subscription.Environment,
            invoice.Amount, invoice.Currency, invoice.CheckoutExpiresAt);
    }
}
