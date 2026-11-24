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

public sealed class PostponePlatformSubscriptionCommandHandler(
    IPlatformBillingRepository persistence,
    IIdentityOperationScope identity,
    PlatformBillingWriteScope writes,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<PostponePlatformSubscriptionCommand, bool>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;
    private IEntityRepository<PlatformSubscription> Subscriptions => persistence.Subscriptions;

    protected override async Task<bool> ExecuteAsync(PostponePlatformSubscriptionCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long id = command.SubscriptionId;

        persistence.ClearTracking();
        using var operation = identity.Begin();
        using var billing = writes.Begin();
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var subscription = await SubscriptionAsync(tenant, id, ct);
        subscription.CheckAfter(Now.AddMinutes(1));
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }

    private Task<PlatformSubscription> SubscriptionAsync(long tenant, long id, CancellationToken ct)
    {
        return Subscriptions.GetRequiredAsync(Subscription => Subscription.Id == id && Subscription.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return data.Enterprises.LockAsync(tenant, ct);
    }
}
