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

public sealed class ListDuePlatformSubscriptionsQueryHandler(
    IPlatformBillingRepository persistence,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListDuePlatformSubscriptionsQuery, List<PlatformBillingKey>>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;

    protected override async Task<List<PlatformBillingKey>> ExecuteAsync(ListDuePlatformSubscriptionsQuery command, CancellationToken ct)
    {
        var now = Now;

        return await persistence.DueAsync(now, ct);
    }
}
