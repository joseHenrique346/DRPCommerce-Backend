using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Billing;

public sealed record StartPlatformBillingRequest(long ContractChangeId);
public sealed record PlatformBillingTeamSelection(long ExpectedContractRevision, IReadOnlyList<long>? RetainedMembershipIds);
public sealed record PlatformBillingNotice(string Code, string Message, DateTime? Deadline);
public sealed record PlatformBillingSummary(long? SubscriptionId, string Status, decimal MonthlyAmount,
    DateTime? PaidUntil, DateTime? GraceEndsAt, string? CheckoutUrl, long? PendingChangeId,
    DateTime? ChangeEffectiveAt, IReadOnlyList<PlatformBillingNotice> Notices);

public sealed class PlatformBillingService(StoreCommerceDbContext db, ICurrentIdentity actor,
    IIdentityOperationScope identity, PlatformBillingWriteScope writes, EnterpriseContractWriteScope grants,
    AccessContractPolicyService access, IPlatformSubscriptionProvider recurring, IPlatformBillingProvider payments,
    IOptions<PlatformBillingOptions> options, TimeProvider time)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;
    private IQueryable<PlatformSubscription> Subscriptions => db.Set<PlatformSubscription>().IgnoreQueryFilters();
    private IQueryable<PlatformBillingInvoice> Invoices => db.Set<PlatformBillingInvoice>().IgnoreQueryFilters();

    public async Task<PlatformBillingSummary> StartAsync(long changeId, CancellationToken ct)
    {
        options.Value.RequireCheckout();
        var tenant = actor.EnterpriseId;
        long subscriptionId;

        using (identity.Begin())
        using (writes.Begin())
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await OwnerAsync(ct);
            var offer = await OfferAsync(changeId, tenant, ct);
            var subscription = await Subscriptions.SingleOrDefaultAsync(Subscription =>
                Subscription.EnterpriseId == tenant && Subscription.ClosedAt == null, ct);

            if (subscription is not null && (subscription.OfferId == offer.Id || subscription.PendingOfferId == offer.Id ||
                await Invoices.AnyAsync(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.OfferId == offer.Id, ct)))
            {
                await tx.CommitAsync(ct);

                return await SummaryCoreAsync(tenant, ct);
            }

            var enterprise = await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == tenant, ct);

            if (offer.Status != EnterpriseContractChangeStatus.Requested || offer.ExpectedContractRevision != enterprise.ContractRevision)
                throw Conflict("Contract.RevisionChanged", "Atualize a escolha e o contrato antes de contratar.");

            await access.ValidateSelectionAsync(tenant, offer.PlanId, offer.AddOnId, offer.RetainedMembershipIds, true, ct);

            if (subscription is null)
            {
                if (enterprise.PlanId != EnterprisePlan.Free.Id || enterprise.AddOnId.HasValue)
                    throw Conflict("Billing.UnmanagedContract", "O contrato legado exige conciliação pela plataforma antes da cobrança.");

                if (Monthly(offer) <= 0)
                    throw Conflict("Billing.PaidOfferRequired", "A loja já utiliza o plano gratuito.");

                subscription = PlatformSubscription.Prepare(offer, options.Value.AccountId, options.Value.Environment, Now);
                db.Add(subscription);
            }
            else
            {
                RequireSnapshot(subscription);
                await ScheduleAsync(subscription, offer, enterprise.ContractRevision, ct);
            }

            offer.BeginBilling();
            await db.SaveChangesAsync(ct);
            subscriptionId = subscription.Id;
            await tx.CommitAsync(ct);
        }

        db.ChangeTracker.Clear();

        try
        {
            await ReconcileAsync(tenant, subscriptionId, ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is not SecurityFailure)
        {
            // A intenção já está persistida. O worker consulta o resultado sem repetir o POST.
            db.ChangeTracker.Clear();
        }

        return await GetAsync(ct);
    }

    public async Task<PlatformBillingSummary> GetAsync(CancellationToken ct)
    {
        using var operation = identity.Begin();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await OwnerAsync(ct);
        var result = await SummaryCoreAsync(actor.EnterpriseId, ct);
        await tx.CommitAsync(ct);

        return result;
    }

    public async Task<object> ListInvoicesAsync(long? beforeId, CancellationToken ct)
    {
        using var operation = identity.Begin();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await OwnerAsync(ct);
        var tenant = actor.EnterpriseId;
        var result = await Invoices.AsNoTracking().Where(Invoice => Invoice.EnterpriseId == tenant &&
                (!beforeId.HasValue || Invoice.Id < beforeId.Value))
            .OrderByDescending(Invoice => Invoice.Id).Take(50).Select(Invoice => new
            {
                Invoice.Id, Invoice.OfferId, Invoice.Amount, Invoice.MonthlyAmount, Invoice.Currency,
                Invoice.IsUpgrade, Invoice.PeriodStart, Invoice.PeriodEnd, Invoice.Status,
                Invoice.AppliedAt, Invoice.ReviewCode
            }).ToListAsync(ct);
        await tx.CommitAsync(ct);

        return result;
    }

    public async Task<PlatformBillingSummary> ReconcileCurrentAsync(CancellationToken ct)
    {
        await GetAsync(ct);
        var tenant = actor.EnterpriseId;
        var id = await Subscriptions.Where(Subscription => Subscription.EnterpriseId == tenant && Subscription.ClosedAt == null)
            .Select(Subscription => (long?)Subscription.Id).SingleOrDefaultAsync(ct);

        if (id.HasValue)
            await ReconcileAsync(tenant, id.Value, ct);

        return await GetAsync(ct);
    }

    public Task<List<PlatformBillingKey>> DueAsync(CancellationToken ct)
    {
        var now = Now;

        return Subscriptions.AsNoTracking().Where(Subscription => Subscription.ClosedAt == null && Subscription.NextCheckAt <= now)
            .OrderBy(Subscription => Subscription.NextCheckAt).ThenBy(Subscription => Subscription.Id).Take(50)
            .Select(Subscription => new PlatformBillingKey(Subscription.EnterpriseId, Subscription.Id)).ToListAsync(ct);
    }

    public async Task ReconcileAsync(long tenant, long id, CancellationToken ct)
    {
        options.Value.RequireAccount();

        try
        {
            using var operation = identity.Begin();
            using var billing = writes.Begin();
            await EnsureSubscriptionAsync(tenant, id, ct);
            await EnsureUpgradeCheckoutAsync(tenant, id, ct);
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await LockAsync(tenant, ct);
            var subscription = await SubscriptionAsync(tenant, id, ct);
            RequireSnapshot(subscription);

            if (subscription.RemoteId is null || subscription.ClosedAt.HasValue)
                return;

            var remote = await recurring.GetAsync(subscription.RemoteId, ct);
            ValidateRemote(subscription, remote);
            subscription.ObserveRemote(remote.Id, remote.CheckoutUrl, remote.Status);
            await ObserveRecurringInvoicesAsync(subscription, ct);
            await ObserveUpgradeAsync(subscription, ct);
            await SynchronizeChangeAsync(subscription, remote, ct);
            await ApplyCurrentRightsAsync(subscription, ct);

            if (subscription.ReviewCode is null)
            {
                foreach (var invoice in db.ChangeTracker.Entries<PlatformBillingInvoice>().Select(Entry => Entry.Entity)
                    .Where(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.Status == "approved" &&
                        Invoice.ReviewCode == null && Invoice.OfferId == subscription.OfferId &&
                        Invoice.PeriodEnd == subscription.PaidUntil))
                    invoice.Applied(Now);
            }

            subscription.CheckAfter(Now.AddMinutes(1));
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        finally
        {
            // Uma indisponibilidade do provedor nunca prolonga os sete dias de carência.
            db.ChangeTracker.Clear();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await EnforceExpiryAsync(tenant, id, timeout.Token);
        }
    }

    private async Task EnsureSubscriptionAsync(long tenant, long id, CancellationToken ct)
    {
        PlatformSubscription subscription;
        EnterpriseContractChange offer;
        string? email = null;
        bool create;

        await using (var tx = await db.Database.BeginTransactionAsync(ct))
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
                var payer = await db.Users.IgnoreQueryFilters().AsNoTracking()
                    .SingleOrDefaultAsync(User => User.Id == offer.RequestedByUserId && User.IsActive, ct);
                email = payer?.Email.Value;

                if (string.IsNullOrWhiteSpace(email))
                    throw Conflict("Billing.PayerRequired", "O responsável pela contratação precisa estar ativo.");

                subscription.Submit(Now);
                await db.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        var remote = create ? await recurring.CreateAsync(subscription.Reference, email!, Monthly(offer), ct) :
            await recurring.RecoverAsync(subscription.Reference, ct);

        if (remote is null)
            return;

        db.ChangeTracker.Clear();
        await using var persist = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        subscription = await SubscriptionAsync(tenant, id, ct);
        ValidateRemote(subscription, remote);

        if (remote.MonthlyAmount != Monthly(offer))
            throw Conflict("Billing.SubscriptionMismatch", "O valor da assinatura não corresponde à oferta.");

        subscription.ObserveRemote(remote.Id, remote.CheckoutUrl, remote.Status);
        await db.SaveChangesAsync(ct);
        await persist.CommitAsync(ct);
    }

    private async Task EnsureUpgradeCheckoutAsync(long tenant, long id, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        PlatformBillingInvoice? invoice;
        PlatformSubscription subscription;
        bool create;

        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            await LockAsync(tenant, ct);
            subscription = await SubscriptionAsync(tenant, id, ct);

            if (!subscription.IsUpgrade || subscription.ReviewCode is not null)
                return;

            invoice = await Invoices.SingleAsync(Invoice => Invoice.SubscriptionId == id &&
                Invoice.OfferId == subscription.PendingOfferId && Invoice.IsUpgrade, ct);

            if (invoice.PreferenceId is not null || invoice.ReviewCode is not null)
                return;

            create = !invoice.SubmittedAt.HasValue;

            if (create && Now >= invoice.CheckoutExpiresAt)
            {
                invoice.ExpireCheckout();
                var current = await OfferAsync(subscription.OfferId, tenant, ct);
                subscription.AbandonUpgrade(current.RetainedMembershipIds);
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return;
            }

            if (create)
            {
                invoice.Submit(Now);
                await db.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        var charge = Charge(subscription, invoice);
        var checkout = create ? await payments.CreateAsync(charge, ct) : await payments.RecoverAsync(charge, ct);

        if (checkout is null)
            return;

        db.ChangeTracker.Clear();
        await using var persist = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        invoice = await Invoices.SingleAsync(Invoice => Invoice.Id == invoice.Id && Invoice.EnterpriseId == tenant, ct);
        invoice.Checkout(checkout.PreferenceId, checkout.CheckoutUrl);
        await db.SaveChangesAsync(ct);
        await persist.CommitAsync(ct);
    }

    private async Task ObserveRecurringInvoicesAsync(PlatformSubscription subscription, CancellationToken ct)
    {
        var remoteInvoices = await recurring.InvoicesAsync(subscription.RemoteId!, ct);

        foreach (var remote in remoteInvoices.OrderBy(Invoice => Invoice.DebitDate))
        {
            var origin = "recurring:" + remote.Id;
            var invoice = await Invoices.SingleOrDefaultAsync(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.Origin == origin, ct);

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
                db.Add(invoice);
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
        var invoices = await Invoices.Where(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.IsUpgrade).ToListAsync(ct);

        foreach (var invoice in invoices)
            await ObserveUpgradeInvoiceAsync(subscription, invoice, ct);
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
            await Invoices.AnyAsync(Invoice => Invoice.PaymentId == payment.Id && Invoice.Id != invoice.Id, ct))
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

        if (!invoice.IsUpgrade && invoice.Status == "approved" && db.ChangeTracker.Entries<PlatformBillingInvoice>().Any(Entry =>
            !ReferenceEquals(Entry.Entity, invoice) && Entry.Entity.SubscriptionId == invoice.SubscriptionId &&
            !Entry.Entity.IsUpgrade && Entry.Entity.Status == "approved" && Entry.Entity.PeriodStart == invoice.PeriodStart))
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
            upgrade = await Invoices.SingleAsync(Invoice => Invoice.SubscriptionId == subscription.Id &&
                Invoice.OfferId == offer.Id && Invoice.IsUpgrade, ct);

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
            await GrantAsync(subscription, null, ct);

            return;
        }

        if (!subscription.PaidUntil.HasValue && subscription.RemoteStatus is "canceled" or "cancelled")
        {
            subscription.Close(Now);

            return;
        }

        if (!subscription.PaidUntil.HasValue || subscription.ReviewCode is not null && !expired)
            return;

        await GrantAsync(subscription, expired ? null : offer, ct);
    }

    private async Task ScheduleAsync(PlatformSubscription subscription, EnterpriseContractChange offer, long revision, CancellationToken ct)
    {
        if (subscription.PendingOfferId.HasValue || subscription.AppliedContractRevision != revision ||
            subscription.ReviewCode is not null && Monthly(offer) != 0)
            throw Conflict("Billing.ChangeUnavailable", "Conclua a operação financeira pendente antes de trocar a oferta.");

        if (!subscription.PaidUntil.HasValue)
        {
            if (Monthly(offer) != 0 || subscription.RemoteId is null)
                throw Conflict("Billing.ChangeUnavailable", "Conclua ou cancele a autorização anterior antes de contratar outra oferta.");

            subscription.Schedule(offer, Now, false);

            return;
        }

        if (Monthly(offer) > 0 && subscription.RemoteStatus != "authorized")
            throw Conflict("Billing.ChangeUnavailable", "Regularize a autorização de renovação antes de trocar a oferta.");

        if (Monthly(offer) > 0 && Now >= subscription.PaidUntil.Value)
            throw Conflict("Billing.CurrentPeriodRequired", "Regularize a mensalidade pendente antes de trocar para outra oferta paga.");

        var current = await OfferAsync(subscription.OfferId, subscription.EnterpriseId, ct);
        var upgrade = Monthly(offer) > Monthly(current);

        if (upgrade && (Now >= subscription.PaidUntil.Value.AddMinutes(-1) ||
            !subscription.PeriodStart.HasValue || Now < subscription.PeriodStart.Value))
            throw Conflict("Billing.CurrentPeriodRequired", "Regularize ou aguarde a renovação antes do upgrade.");

        subscription.Schedule(offer, upgrade ? Now : subscription.PaidUntil.Value, upgrade);

        if (!upgrade)
            return;

        var amount = PlatformBillingPolicy.UpgradeAmount(Monthly(current), Monthly(offer),
            subscription.PeriodStart!.Value, subscription.PaidUntil.Value, Now);
        db.Add(PlatformBillingInvoice.Create(subscription, offer, "upgrade:" + offer.Id,
            true, amount, Now, subscription.PaidUntil.Value,
            new[] { Now.AddMinutes(30), subscription.PaidUntil.Value }.Min()));
    }

    public async Task EnforceExpiryAsync(long tenant, long id, CancellationToken ct)
    {
        using var operation = identity.Begin();
        using var billing = writes.Begin();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var subscription = await SubscriptionAsync(tenant, id, ct);

        if (subscription.PaidUntil.HasValue && Now >= subscription.PaidUntil.Value.AddDays(PlatformBillingPolicy.GraceDays))
        {
            await GrantAsync(subscription, null, ct);
            await db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    private async Task GrantAsync(PlatformSubscription subscription, EnterpriseContractChange? offer, CancellationToken ct)
    {
        var enterprise = await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == subscription.EnterpriseId, ct);

        if (enterprise.ContractRevision != subscription.AppliedContractRevision)
        {
            subscription.Review("Billing.ContractChanged");

            return;
        }

        var plan = offer?.PlanId ?? EnterprisePlan.Free.Id;
        var addOn = offer?.AddOnId;

        if (enterprise.PlanId == plan && enterprise.AddOnId == addOn)
            return;

        var retained = plan == EnterprisePlan.Free.Id ? [] : subscription.RetainedMembershipIds;
        await access.ValidateSelectionAsync(subscription.EnterpriseId, plan, addOn, retained, true, ct);
        using var grant = grants.Begin(subscription.EnterpriseId, retained);
        enterprise.ChangeContract(plan, addOn);
        subscription.Applied(enterprise.ContractRevision);
        await db.SaveChangesAsync(ct);
    }

    private async Task<PlatformBillingSummary> SummaryCoreAsync(long tenant, CancellationToken ct)
    {
        var subscription = await Subscriptions.AsNoTracking().Where(Subscription => Subscription.EnterpriseId == tenant)
            .OrderByDescending(Subscription => Subscription.Id).FirstOrDefaultAsync(ct);

        if (subscription is null)
            return new PlatformBillingSummary(null, "free", 0, null, null, null, null, null, []);

        var offer = await OfferAsync(subscription.OfferId, tenant, ct);
        var status = PlatformBillingPolicy.Status(subscription.PaidUntil, subscription.ClosedAt.HasValue, subscription.ReviewCode is not null, Now);
        var notices = new List<PlatformBillingNotice>();
        var deadline = subscription.PaidUntil?.AddDays(PlatformBillingPolicy.GraceDays);

        if (status == "past_due")
            notices.Add(new("Billing.PaymentDue", "Pagamento pendente. Regularize até o fim da carência para manter os recursos do plano.", deadline));

        if (status == "expired")
            notices.Add(new("Billing.GraceExpired", "A carência terminou. A loja utiliza os recursos do Gratuito até a regularização.", deadline));

        if (subscription.ReviewCode is not null)
            notices.Add(new(subscription.ReviewCode, "Há uma pendência financeira que precisa de revisão pela plataforma.", deadline));

        var checkout = subscription.CheckoutUrl;

        if (subscription.IsUpgrade)
            checkout = await Invoices.AsNoTracking().Where(Invoice => Invoice.SubscriptionId == subscription.Id &&
                Invoice.OfferId == subscription.PendingOfferId && Invoice.IsUpgrade && Invoice.CheckoutExpiresAt > Now &&
                Invoice.Status != "approved" && Invoice.ReviewCode == null).Select(Invoice => Invoice.CheckoutUrl).SingleOrDefaultAsync(ct);

        return new PlatformBillingSummary(subscription.Id, status, Monthly(offer), subscription.PaidUntil, deadline,
            checkout, subscription.PendingOfferId, subscription.ChangeEffectiveAt, notices);
    }

    private async Task OwnerAsync(CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var user = actor.UserId;
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM usuario WHERE id = {user} FOR UPDATE", ct);
        await LockAsync(tenant, ct);

        if (!await db.EnterpriseMemberships.IgnoreQueryFilters().AnyAsync(Membership => Membership.Id == actor.MembershipId &&
            Membership.EnterpriseId == tenant && Membership.UserId == user && Membership.KindId == 1 && Membership.IsActive &&
            Membership.User.IsActive && Membership.Enterprise.IsActive, ct))
            throw new SecurityFailure(403, "Billing.OwnerRequired", "Apenas o dono atual da loja pode gerir a assinatura.");
    }

    private async Task<EnterpriseContractChange> OfferAsync(long id, long tenant, CancellationToken ct)
    {
        return await db.EnterpriseContractChanges.IgnoreQueryFilters().SingleOrDefaultAsync(Offer => Offer.Id == id && Offer.EnterpriseId == tenant, ct)
            ?? throw new SecurityFailure(404, "Billing.OfferNotFound", "Escolha não encontrada.");
    }

    public async Task PostponeAsync(long tenant, long id, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        using var operation = identity.Begin();
        using var billing = writes.Begin();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var subscription = await SubscriptionAsync(tenant, id, ct);
        subscription.CheckAfter(Now.AddMinutes(1));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task SelectTeamAsync(PlatformBillingTeamSelection request, CancellationToken ct)
    {
        using var operation = identity.Begin();
        using var billing = writes.Begin();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await OwnerAsync(ct);
        var tenant = actor.EnterpriseId;
        var subscription = await Subscriptions.SingleOrDefaultAsync(Subscription =>
            Subscription.EnterpriseId == tenant && Subscription.ClosedAt == null, ct)
            ?? throw Conflict("Billing.NoPendingChange", "Não há assinatura pendente.");
        var enterprise = await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == tenant, ct);

        if (!subscription.PendingOfferId.HasValue || subscription.IsUpgrade || enterprise.ContractRevision != request.ExpectedContractRevision)
            throw Conflict("Billing.NoPendingChange", "Consulte a revisão e a mudança agendada antes de escolher a equipe.");

        var offer = await OfferAsync(subscription.PendingOfferId.Value, tenant, ct);
        var selection = await access.ValidateSelectionAsync(tenant, offer.PlanId, offer.AddOnId, request.RetainedMembershipIds, true, ct);
        subscription.SelectTeam(selection);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private Task<PlatformSubscription> SubscriptionAsync(long tenant, long id, CancellationToken ct)
    {
        return Subscriptions.SingleAsync(Subscription => Subscription.Id == id && Subscription.EnterpriseId == tenant, ct);
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
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

    private static PlatformBillingCharge Charge(PlatformSubscription subscription, PlatformBillingInvoice invoice)
    {
        return new PlatformBillingCharge(invoice.Reference, subscription.AccountId, subscription.Environment,
            invoice.Amount, invoice.Currency, invoice.CheckoutExpiresAt);
    }

    private static decimal Monthly(EnterpriseContractChange offer)
    {
        return offer.PlanMonthlyPrice + offer.AddOnMonthlyPrice;
    }

    private static SecurityFailure Conflict(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }
}

public sealed record PlatformBillingKey(long EnterpriseId, long Id);
