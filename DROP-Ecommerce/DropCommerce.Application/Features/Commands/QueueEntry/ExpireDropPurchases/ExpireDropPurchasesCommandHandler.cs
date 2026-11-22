using System.Security.Cryptography;
using System.Text;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;
using DropCommerce.Application.Security;

namespace DropCommerce.Application.Features.Commands;

public sealed class ExpireDropPurchasesCommandHandler(
    IDropQueueRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    TimeProvider clock)
    : IRequestHandler<ExpireDropPurchasesCommand, Result<bool>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<bool>> Handle(ExpireDropPurchasesCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long eventId = command.EventId;

        persistence.ClearTracking();
        using var operation = writes.Begin("expiration", tenant);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var reservations = await persistence.ListExpiredReservationsAsync(eventId, tenant, Now, ct);

        foreach (var reservation in reservations)
        {
            foreach (var line in reservation.Lines)
                line.Variant.Release(line.Quantity);

            reservation.Expire(Now);
        }

        var expiredIds = reservations.Select(Reservation => Reservation.Id).ToList();
        var orders = await persistence.ListExpiredOrdersAsync(expiredIds, tenant, ct);

        foreach (var order in orders)
            order.ExpireUnpaid();

        var sessions = await persistence.ListExpiredSessionsAsync(eventId, tenant, Now, ct);

        foreach (var session in sessions)
        {
            session.Expire();
            session.QueueEntry.Expire(Now);
        }

        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<bool>.Success(true);
    }
}
