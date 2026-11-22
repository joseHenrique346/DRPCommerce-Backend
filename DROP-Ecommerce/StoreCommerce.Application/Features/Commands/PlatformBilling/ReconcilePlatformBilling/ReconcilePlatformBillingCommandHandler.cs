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

public sealed class ReconcilePlatformBillingCommandHandler(
    IPlatformBillingRepository persistence,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ReconcilePlatformBillingCommand, PlatformBillingSummary>(context)
{
    private IEntityRepository<PlatformSubscription> Subscriptions => persistence.Subscriptions;

    protected override async Task<PlatformBillingSummary> ExecuteAsync(ReconcilePlatformBillingCommand command, CancellationToken ct)
    {
        await mediator.SendRequiredAsync(new GetPlatformBillingQuery(), ct);
        var tenant = actor.EnterpriseId;
        var id = await Subscriptions.FindValueAsync(Subscription => Subscription.EnterpriseId == tenant && Subscription.ClosedAt == null,
                Subscription => (long?)Subscription.Id, ct, ignoreQueryFilters: true);

        if (id.HasValue)
            await mediator.SendRequiredAsync(new ReconcilePlatformSubscriptionCommand(tenant, id.Value), ct);

        return await mediator.SendRequiredAsync(new GetPlatformBillingQuery(), ct);
    }
}
