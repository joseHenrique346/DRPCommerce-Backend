using StoreCommerce.Application.Interfaces.Billing;
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

public sealed class ApplyPlatformSubscriptionRightsCommandHandler(
    IPlatformBillingRepository persistence,
    EnterpriseContractWriteScope grants,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ApplyPlatformSubscriptionRightsCommand, bool>(context)
{
    private IEntityRepository<PlatformSubscription> Subscriptions => persistence.Subscriptions;

    protected override async Task<bool> ExecuteAsync(ApplyPlatformSubscriptionRightsCommand command, CancellationToken ct)
    {
        PlatformSubscription subscription = await persistence.Subscriptions.GetRequiredAsync(Subscription => Subscription.EnterpriseId == command.Tenant && Subscription.Id == command.SubscriptionId, ct, ignoreQueryFilters: true);
        EnterpriseContractChange? offer = command.OfferId.HasValue ? await persistence.Offers.GetRequiredAsync(Offer => Offer.Id == command.OfferId.Value && Offer.EnterpriseId == command.Tenant, ct, ignoreQueryFilters: true) : null;

        var enterprise = await data.Enterprises.GetRequiredAsync(Enterprise => Enterprise.Id == subscription.EnterpriseId, ct, ignoreQueryFilters: true);

        if (enterprise.ContractRevision != subscription.AppliedContractRevision)
        {
            subscription.Review("Billing.ContractChanged");

            return true;
        }

        var plan = offer?.PlanId ?? EnterprisePlan.Free.Id;
        var addOn = offer?.AddOnId;

        if (enterprise.PlanId == plan && enterprise.AddOnId == addOn)
            return true;

        var retained = plan == EnterprisePlan.Free.Id ? [] : subscription.RetainedMembershipIds;
        await mediator.SendRequiredAsync(new ValidateEnterpriseTeamSelectionQuery(subscription.EnterpriseId, plan, addOn, retained, true), ct);
        using var grant = grants.Begin(subscription.EnterpriseId, retained);
        enterprise.ChangeContract(plan, addOn);
        subscription.Applied(enterprise.ContractRevision);
        await unitOfWork.CommitAsync(ct);

        return true;
    }
}
