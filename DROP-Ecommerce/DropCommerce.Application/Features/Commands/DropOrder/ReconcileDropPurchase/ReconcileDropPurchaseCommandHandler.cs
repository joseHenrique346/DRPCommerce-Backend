using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReconcileDropPurchaseCommandHandler(
    IDropCheckoutRepository persistence,
    IStoreDropPayments payments,
    TimeProvider clock,
    IMediator mediator)
    : IRequestHandler<ReconcileDropPurchaseCommand, Result<bool>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<bool>> Handle(ReconcileDropPurchaseCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long orderId = command.OrderId;

        var order = await persistence.GetOrderWithReservationAsync(orderId, tenant, ct);

        if (!order.PaymentOperationId.HasValue)
            return Result<bool>.Success(true);

        var receipt = !order.PaymentIntentAccepted && Now < order.DropReservation.ExpiresAt
            ? await payments.StartAsync(tenant, new(order.PaymentOperationId.Value, order.Id, order.CustomerId,
                order.TotalAmount, "BRL", order.DropReservation.ExpiresAt), ct)
            : await payments.GetAsync(tenant, order.PaymentOperationId.Value, ct);

        if (receipt is not null)
            await mediator.SendRequiredAsync(new ApplyDropPaymentReceiptCommand(receipt), ct);

        return Result<bool>.Success(true);
    }
}
