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

public sealed class SelectPlatformBillingTeamCommandHandler(
    IPlatformBillingRepository persistence,
    IIdentityOperationScope identity,
    PlatformBillingWriteScope writes,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<SelectPlatformBillingTeamCommand, bool>(context)
{
    private IEntityRepository<PlatformSubscription> Subscriptions => persistence.Subscriptions;

    protected override async Task<bool> ExecuteAsync(SelectPlatformBillingTeamCommand command, CancellationToken ct)
    {
        PlatformBillingTeamSelection request = command.Input;

        using var operation = identity.Begin();
        using var billing = writes.Begin();
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await OwnerAsync(ct);
        var tenant = actor.EnterpriseId;
        var subscription = await Subscriptions.FindAsync(Subscription =>
            Subscription.EnterpriseId == tenant && Subscription.ClosedAt == null, ct, ignoreQueryFilters: true)
            ?? throw Conflict("Billing.NoPendingChange", "Não há assinatura pendente.");
        var enterprise = await data.Enterprises.GetRequiredAsync(Enterprise => Enterprise.Id == tenant, ct, ignoreQueryFilters: true);

        if (!subscription.PendingOfferId.HasValue || subscription.IsUpgrade || enterprise.ContractRevision != request.ExpectedContractRevision)
            throw Conflict("Billing.NoPendingChange", "Consulte a revisão e a mudança agendada antes de escolher a equipe.");

        var offer = await OfferAsync(subscription.PendingOfferId.Value, tenant, ct);
        var selection = await mediator.SendRequiredAsync(new ValidateEnterpriseTeamSelectionQuery(tenant, offer.PlanId, offer.AddOnId, request.RetainedMembershipIds, true), ct);
        subscription.SelectTeam(selection);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
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

    private static SecurityFailure Conflict(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return data.Enterprises.LockAsync(tenant, ct);
    }
}
