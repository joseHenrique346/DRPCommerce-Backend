using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AcknowledgeDropPaymentCommandHandler(
    IDropSalePaymentRepository persistence,
    IIdentityOperationScope identity,
    DropSalePaymentWriteScope writes,
    IdentityHandlerContext context)
    : IdentityHandlerBase<AcknowledgeDropPaymentCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(AcknowledgeDropPaymentCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        Guid operationId = command.OperationId;
        long revision = command.Revision;
        bool fulfillmentClosed = command.FulfillmentClosed;
        bool requiresReview = command.RequiresReview;

        using var operation = identity.Begin();
        using var payment = writes.Begin(tenant);
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var intent = await ReadAsync(tenant, operationId, ct);
        intent.Acknowledge(revision);

        if (requiresReview)
            intent.RequireFulfillmentReview();

        if (fulfillmentClosed)
            intent.CloseFulfillment();

        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }

    private async Task<DropSalePaymentIntent> ReadAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        return await persistence.FindIntentAsync(tenant, operationId, ct)
            ?? throw new SecurityFailure(404, "DropPayment.IntentNotFound", "Intenção de pagamento não encontrada.");
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return persistence.LockTenantAsync(tenant, ct);
    }
}
