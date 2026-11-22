using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListDuePlatformBillingNotificationsQueryHandler(
    IPlatformBillingNotificationRepository persistence,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListDuePlatformBillingNotificationsQuery, List<long>>(context)
{
    protected override async Task<List<long>> ExecuteAsync(ListDuePlatformBillingNotificationsQuery command, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        return await persistence.DueAsync(now, ct);
    }
}
