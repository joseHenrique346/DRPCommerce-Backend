using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartDropSalePaymentCommandHandler(
    IDropSalePaymentRepository persistence,
    IIdentityOperationScope identity,
    DropSalePaymentWriteScope writes,
    IDropSaleGateway gateway,
    TimeProvider clock,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<StartDropSalePaymentCommand, DropSalePaymentReceipt>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;

    protected override async Task<DropSalePaymentReceipt> ExecuteAsync(StartDropSalePaymentCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        StartDropSalePayment request = command.Input;

        var existing = await persistence.ReadIntentAsync(tenant, request.OperationId, ct);

        if (existing is not null)
        {
            if (existing.DropOrderId != request.DropOrderId || existing.CustomerId != request.CustomerId ||
                existing.Amount != request.Amount || existing.ExpiresAt != request.ExpiresAt || existing.Currency != request.Currency)
                throw new SecurityFailure(409, "DropPayment.IdempotencyConflict", "A operação já identifica outra cobrança.");

            return await mediator.SendRequiredAsync(new ReconcileDropSalePaymentCommand(tenant, request.OperationId), ct);
        }

        if (request.ExpiresAt <= Now || request.ExpiresAt > Now.AddMinutes(15))
            throw new SecurityFailure(409, "DropPayment.InvalidDeadline", "O prazo da cobrança deve respeitar a sessão de compra.");

        var context = await gateway.ContextAsync(tenant, ct);
        await mediator.SendRequiredAsync(new GetDropBuyerProfileQuery(tenant, request.CustomerId), ct);

        using (identity.Begin())
        using (writes.Begin(tenant))
        {
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);
            await persistence.LockCustomerAndTenantAsync(tenant, request.CustomerId, ct);
            existing = await persistence.FindIntentAsync(tenant, request.OperationId, ct);

            if (existing is not null && (existing.DropOrderId != request.DropOrderId || existing.CustomerId != request.CustomerId ||
                existing.Amount != request.Amount || existing.ExpiresAt != request.ExpiresAt || existing.Currency != request.Currency))
                throw new SecurityFailure(409, "DropPayment.IdempotencyConflict", "A operação já identifica outra cobrança.");

            if (existing is null)
            {
                await mediator.SendRequiredAsync(new GetDropBuyerProfileQuery(tenant, request.CustomerId), ct);
                persistence.Add(DropSalePaymentIntent.Prepare(tenant, request.OperationId, request.DropOrderId, request.CustomerId,
                    request.Amount, request.ExpiresAt, context.Provider, context.Environment, context.ExternalAccountId,
                    context.ProviderConnectionId, Now));
                await unitOfWork.CommitAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        persistence.ClearTracking();

        return await mediator.SendRequiredAsync(new ReconcileDropSalePaymentCommand(tenant, request.OperationId), ct);
    }
}
