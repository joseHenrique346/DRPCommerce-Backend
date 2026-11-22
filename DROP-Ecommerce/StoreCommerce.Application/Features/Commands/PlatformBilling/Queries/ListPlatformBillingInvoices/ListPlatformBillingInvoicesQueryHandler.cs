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

public sealed class ListPlatformBillingInvoicesQueryHandler(
    IPlatformBillingRepository persistence,
    IIdentityOperationScope identity,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListPlatformBillingInvoicesQuery, IReadOnlyList<PlatformBillingInvoiceResponse>>(context)
{
    protected override async Task<IReadOnlyList<PlatformBillingInvoiceResponse>> ExecuteAsync(ListPlatformBillingInvoicesQuery command, CancellationToken ct)
    {
        long? beforeId = command.BeforeId;

        using var operation = identity.Begin();
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await OwnerAsync(ct);
        var tenant = actor.EnterpriseId;
        var result = await persistence.ListInvoicesAsync(tenant, beforeId, ct);
        await tx.CommitAsync(ct);

        return result;
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

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return data.Enterprises.LockAsync(tenant, ct);
    }
}
