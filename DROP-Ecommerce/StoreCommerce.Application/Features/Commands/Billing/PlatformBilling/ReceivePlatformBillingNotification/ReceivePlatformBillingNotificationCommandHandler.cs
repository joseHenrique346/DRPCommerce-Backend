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

public sealed class ReceivePlatformBillingNotificationCommandHandler(
    IPlatformBillingNotificationRepository persistence,
    IPlatformBillingSignature signature,
    PlatformBillingWriteScope writes,
    IIdentityOperationScope identity,
    IOptions<PlatformBillingOptions> options,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ReceivePlatformBillingNotificationCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(ReceivePlatformBillingNotificationCommand command, CancellationToken ct)
    {
        string topic = command.Topic;
        string id = command.ResourceId;
        string requestId = command.RequestId;
        string signatureHeader = command.Signature;

        var delivery = signature.Verify(id, requestId, signatureHeader);
        var notification = PlatformBillingNotification.Receive(delivery, topic, id, options.Value.AccountId,
            options.Value.Environment, clock.GetUtcNow().UtcDateTime);
        using var operation = identity.Begin();
        using var financial = writes.Begin();
        await persistence.AddDeliveryAsync(notification, ct);

        return true;
    }
}
