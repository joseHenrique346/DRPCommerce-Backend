using StoreCommerce.Application.Interfaces.Billing;
using StoreCommerce.Application.Interfaces.Identity;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RetryPlatformBillingNotificationCommandHandler(
    IPlatformBillingNotificationRepository persistence,
    PlatformBillingWriteScope writes,
    IIdentityOperationScope identity,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<RetryPlatformBillingNotificationCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(RetryPlatformBillingNotificationCommand command, CancellationToken ct)
    {
        long id = command.NotificationId;

        persistence.ClearTracking();
        using var operation = identity.Begin();
        using var financial = writes.Begin();
        var stored = await persistence.Notifications.GetRequiredAsync(Notification => Notification.Id == id, ct);
        stored.Retry(clock.GetUtcNow().UtcDateTime.AddMinutes(1));
        await persistence.SaveChangesAsync(ct);

        return true;
    }
}
