using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Application.Interfaces.Orders;
using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class CheckoutDropPurchaseCommandHandler(
    IDropCheckoutRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    IStoreDropPayments payments,
    ICurrentDropActor actor,
    TimeProvider clock,
    IMediator mediator)
    : IRequestHandler<CheckoutDropPurchaseCommand, Result<DropPurchaseResponse>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    private long Customer => actor.CustomerId;

    public async Task<Result<DropPurchaseResponse>> Handle(CheckoutDropPurchaseCommand command, CancellationToken ct)
    {
        long reservationId = command.ReservationId;

        var tenant = actor.EnterpriseId;
        var customerId = Customer;
        var profile = await payments.ProfileAsync(tenant, customerId, ct);

        if (new[] { profile.AddressLine, profile.City, profile.State, profile.ZipCode }.Any(string.IsNullOrWhiteSpace))
            throw new SecurityFailure(409, "DropPurchase.AddressRequired", "Complete o endereço antes de iniciar a cobrança.");

        long orderId;

        using (writes.Begin("sales", tenant))
        {
            await using var tx = await persistence.BeginTransactionAsync(ct);
            await persistence.LockCommercialTenantAsync(tenant, ct);
            var reservation = await persistence.FindReservationAsync(reservationId, customerId, tenant, ct)
                ?? throw new SecurityFailure(404, "DropPurchase.ReservationNotFound", "Reserva não encontrada.");
            var order = await persistence.FindReservationOrderAsync(reservationId, customerId, tenant, ct);

            if (order is null)
            {
                if (reservation.DropReservationStatusId != 1 || Now >= reservation.ExpiresAt || !reservation.QueueSessionId.HasValue || reservation.Lines.Count == 0)
                    throw new SecurityFailure(409, "DropPurchase.ReservationExpired", "A reserva não está disponível para iniciar cobrança.");

                order = DropOrder.Create(reservation.DropEventId, customerId, reservation.Id, null, 1, 1,
                    reservation.TotalAmount, 0, 0, 0, reservation.TotalAmount,
                    profile.AddressLine, profile.City, profile.State, profile.ZipCode, null);
                order.PreparePayment(profile.Name, profile.Email);
                persistence.Add(order);
                await unitOfWork.CommitAsync(ct);

                foreach (var line in reservation.Lines)
                {
                    var item = DropOrderItem.Create(order.Id, reservation.DropProductId, line.ItemName, line.SKU,
                        line.Quantity, line.UnitPrice, line.Quantity * line.UnitPrice);
                    item.BindVariantSnapshot(line.VariantId, line.OptionsJson);
                    persistence.Add(item);
                }

                await unitOfWork.CommitAsync(ct);
            }

            orderId = order.Id;
            await tx.CommitAsync(ct);
        }

        persistence.ClearTracking();
        await mediator.SendRequiredAsync(new ReconcileDropPurchaseCommand(tenant, orderId), ct);

        return Result<DropPurchaseResponse>.Success(await mediator.SendRequiredAsync(new GetDropPurchaseQuery(orderId), ct));
    }
}
