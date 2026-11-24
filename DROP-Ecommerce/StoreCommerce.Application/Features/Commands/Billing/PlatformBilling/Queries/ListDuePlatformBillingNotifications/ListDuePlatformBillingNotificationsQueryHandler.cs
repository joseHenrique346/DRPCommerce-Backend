using StoreCommerce.Application.Interfaces.Billing;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
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
