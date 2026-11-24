using StoreCommerce.Application.Interfaces.Billing;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Enums.Commercial;
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

public sealed class StartPlatformBillingCommandHandler(
    IPlatformBillingRepository persistence,
    IIdentityOperationScope identity,
    PlatformBillingWriteScope writes,
    IOptions<PlatformBillingOptions> options,
    TimeProvider clock,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<StartPlatformBillingCommand, PlatformBillingSummary>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;
    private IEntityRepository<PlatformSubscription> Subscriptions => persistence.Subscriptions;
    private IEntityRepository<PlatformBillingInvoice> Invoices => persistence.Invoices;

    protected override async Task<PlatformBillingSummary> ExecuteAsync(StartPlatformBillingCommand command, CancellationToken ct)
    {
        long changeId = command.Input.ContractChangeId;

        options.Value.RequireCheckout();
        var tenant = actor.EnterpriseId;
        long subscriptionId;

        using (identity.Begin())
        using (writes.Begin())
        {
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);
            await OwnerAsync(ct);
            var offer = await OfferAsync(changeId, tenant, ct);
            var subscription = await Subscriptions.FindAsync(Subscription =>
                Subscription.EnterpriseId == tenant && Subscription.ClosedAt == null, ct, ignoreQueryFilters: true);

            if (subscription is not null && (subscription.OfferId == offer.Id || subscription.PendingOfferId == offer.Id ||
                await Invoices.ExistsAsync(Invoice => Invoice.SubscriptionId == subscription.Id && Invoice.OfferId == offer.Id, ct, ignoreQueryFilters: true)))
            {
                await tx.CommitAsync(ct);

                return await mediator.SendRequiredAsync(new GetPlatformBillingSummaryQuery(tenant), ct);
            }

            var enterprise = await data.Enterprises.GetRequiredAsync(Enterprise => Enterprise.Id == tenant, ct, ignoreQueryFilters: true);

            if (offer.Status != EnterpriseContractChangeStatus.Requested || offer.ExpectedContractRevision != enterprise.ContractRevision)
                throw Conflict("Contract.RevisionChanged", "Atualize a escolha e o contrato antes de contratar.");

            await mediator.SendRequiredAsync(new ValidateEnterpriseTeamSelectionQuery(tenant, offer.PlanId, offer.AddOnId, offer.RetainedMembershipIds, true), ct);

            if (subscription is null)
            {
                if (enterprise.PlanId != EnterprisePlan.Free.Id || enterprise.AddOnId.HasValue)
                    throw Conflict("Billing.UnmanagedContract", "O contrato legado exige conciliação pela plataforma antes da cobrança.");

                if (Monthly(offer) <= 0)
                    throw Conflict("Billing.PaidOfferRequired", "A loja já utiliza o plano gratuito.");

                subscription = PlatformSubscription.Prepare(offer, options.Value.AccountId, options.Value.Environment, Now);
                Subscriptions.Add(subscription);
            }
            else
            {
                RequireSnapshot(subscription);
                await ScheduleAsync(subscription, offer, enterprise.ContractRevision, ct);
            }

            offer.BeginBilling();
            await unitOfWork.CommitAsync(ct);
            subscriptionId = subscription.Id;
            await tx.CommitAsync(ct);
        }

        persistence.ClearTracking();

        try
        {
            await mediator.SendRequiredAsync(new ReconcilePlatformSubscriptionCommand(tenant, subscriptionId), ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is not SecurityFailure)
        {
            // A intenção já está persistida. O worker consulta o resultado sem repetir o POST.
            persistence.ClearTracking();
        }

        return await mediator.SendRequiredAsync(new GetPlatformBillingQuery(), ct);
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
        Invoices.Add(PlatformBillingInvoice.Create(subscription, offer, "upgrade:" + offer.Id,
            true, amount, Now, subscription.PaidUntil.Value,
            new[] { Now.AddMinutes(30), subscription.PaidUntil.Value }.Min()));
    }

    private async Task OwnerAsync(CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var user = actor.UserId;
        await data.Users.LockAsync(user, ct);
        await LockAsync(tenant, ct);

        if (!await data.EnterpriseMemberships.ExistsAsync(Membership => Membership.Id == actor.MembershipId &&
            Membership.EnterpriseId == tenant && Membership.UserId == user && Membership.KindId == 1 && Membership.IsActive &&
            Membership.User.IsActive && Membership.Enterprise.IsActive, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(403, "Billing.OwnerRequired", "Apenas o dono atual da loja pode gerir a assinatura.");
    }

    private async Task<EnterpriseContractChange> OfferAsync(long id, long tenant, CancellationToken ct)
    {
        return await persistence.Offers.FindAsync(Offer => Offer.Id == id && Offer.EnterpriseId == tenant, ct, ignoreQueryFilters: true)
            ?? throw new SecurityFailure(404, "Billing.OfferNotFound", "Escolha não encontrada.");
    }

    private void RequireSnapshot(PlatformSubscription subscription)
    {
        if (subscription.AccountId != options.Value.AccountId || subscription.Environment != options.Value.Environment)
            throw Conflict("Billing.AccountChanged", "Restaure o recebedor e o ambiente originais da assinatura.");
    }

    private static decimal Monthly(EnterpriseContractChange offer)
    {
        return offer.PlanMonthlyPrice + offer.AddOnMonthlyPrice;
    }

    private static SecurityFailure Conflict(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return data.Enterprises.LockAsync(tenant, ct);
    }
}
