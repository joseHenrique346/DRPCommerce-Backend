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

public sealed class ReconcileDropSalePaymentCommandHandler(
    IDropSalePaymentRepository persistence,
    IIdentityOperationScope identity,
    DropSalePaymentWriteScope writes,
    IDropSaleGateway gateway,
    TimeProvider clock,
    IMediator mediator,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ReconcileDropSalePaymentCommand, DropSalePaymentReceipt>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;

    protected override async Task<DropSalePaymentReceipt> ExecuteAsync(ReconcileDropSalePaymentCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        Guid operationId = command.OperationId;

        using var operation = identity.Begin();
        using var payment = writes.Begin(tenant);
        DropSalePaymentIntent intent;
        bool create;

        await using (var tx = await unitOfWork.BeginTransactionAsync(ct))
        {
            await LockAsync(tenant, ct);
            intent = await ReadAsync(tenant, operationId, ct);
            create = !intent.SubmittedAt.HasValue;

            if (create && Now >= intent.ExpiresAt)
            {
                intent.Observe("not_submitted", null);
                await unitOfWork.CommitAsync(ct);
                await tx.CommitAsync(ct);

                return Receipt(intent);
            }

            if (create)
            {
                intent.Submit(Now);
                await unitOfWork.CommitAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        if (intent.Status == "not_submitted")
            return Receipt(intent);

        var profile = create ? await mediator.SendRequiredAsync(new GetDropBuyerProfileQuery(tenant, intent.CustomerId), ct) : null;
        var request = new PaymentRequest(tenant, intent.DropOrderId, intent.CustomerId, profile?.Name ?? "Comprador DROP",
            profile?.Email ?? "", intent.Amount, intent.Currency, 1, intent.ExpiresAt, intent.OperationId.ToString("N"),
            new PaymentProviderContext(intent.Provider, intent.Environment, intent.ExternalAccountId, intent.ProviderConnectionId), intent.ExternalReference, "drop");
        PaymentSession? checkout = null;
        DropSaleGatewayResult? result = null;

        if (intent.PreferenceId is null)
        {
            try
            {
                checkout = await gateway.CreateAsync(request, !create, ct);
            }
            catch (PaymentNotSubmittedException) when (create)
            {
                result = new DropSaleGatewayResult("not_submitted", null);
            }
        }

        if (intent.PreferenceId is not null || checkout is not null)
            result = await gateway.ReadAsync(request, ct);
        persistence.ClearTracking();
        await using var persist = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        intent = await ReadAsync(tenant, operationId, ct);

        if (checkout is not null)
            intent.Checkout(checkout.PreferenceId ?? checkout.Reference, checkout.CheckoutUrl);

        if (result is not null)
            intent.Observe(result.Status, result.PaymentId);

        intent.CheckAfter(Now.AddMinutes(1));
        await unitOfWork.CommitAsync(ct);
        await persist.CommitAsync(ct);

        return Receipt(intent);
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

    private static DropSalePaymentReceipt Receipt(DropSalePaymentIntent intent)
    {
        return new DropSalePaymentReceipt(intent.OperationId, intent.EnterpriseId, intent.DropOrderId, intent.CustomerId,
            intent.Amount, intent.Currency, intent.Status, intent.CheckoutUrl, intent.PaymentId, intent.Revision);
    }
}
