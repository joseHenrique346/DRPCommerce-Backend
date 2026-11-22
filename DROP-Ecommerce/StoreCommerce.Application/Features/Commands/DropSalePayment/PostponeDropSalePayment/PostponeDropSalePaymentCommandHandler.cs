using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PostponeDropSalePaymentCommandHandler(
    IDropSalePaymentRepository persistence,
    IIdentityOperationScope identity,
    DropSalePaymentWriteScope writes,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<PostponeDropSalePaymentCommand, bool>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;

    protected override async Task<bool> ExecuteAsync(PostponeDropSalePaymentCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        Guid operationId = command.OperationId;

        persistence.ClearTracking();
        using var operation = identity.Begin();
        using var payment = writes.Begin(tenant);
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var intent = await ReadAsync(tenant, operationId, ct);
        intent.CheckAfter(Now.AddMinutes(1));
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
