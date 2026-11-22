using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Domain.Interfaces;
using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetPlatformBillingSummaryQueryHandler(
    IPlatformBillingRepository persistence,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<GetPlatformBillingSummaryQuery, PlatformBillingSummary>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;

    protected override async Task<PlatformBillingSummary> ExecuteAsync(GetPlatformBillingSummaryQuery command, CancellationToken ct)
    {
        long tenant = command.Tenant;

        var subscription = await persistence.LatestSubscriptionAsync(tenant, ct);

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
            checkout = await persistence.CheckoutUrlAsync(subscription.Id, subscription.PendingOfferId, Now, ct);

        return new PlatformBillingSummary(subscription.Id, status, Monthly(offer), subscription.PaidUntil, deadline,
            checkout, subscription.PendingOfferId, subscription.ChangeEffectiveAt, notices);
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
}
