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

public sealed class AdmitDropQueueCommandHandler(
    IDropQueueRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    IStoreCustomerReader customers,
    IOptions<DropQueueOptions> options,
    TimeProvider clock,
    StoreIntegrationScope integration,
    IMediator mediator)
    : IRequestHandler<AdmitDropQueueCommand, Result<bool>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<bool>> Handle(AdmitDropQueueCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long eventId = command.EventId;

        using var storeOperation = integration.Begin(tenant);
        await mediator.SendRequiredAsync(new ExpireDropPurchasesCommand(tenant, eventId), ct);
        persistence.ClearTracking();
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop)
            return Result<bool>.Success(true);

        using var operation = writes.Begin("sales", tenant);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var drop = await persistence.GetEventAsync(eventId, tenant, ct);

        if (!DropEventPublication.IsPublished(drop) || Now < drop.DropStartsAt || Now >= drop.DropEndsAt || !await HasProductsAsync(tenant, eventId, ct))
            return Result<bool>.Success(true);

        var expired = await persistence.ListExpiredEntriesAsync(eventId, ct);
        var position = await TailAsync(eventId, ct);

        foreach (var entry in expired)
            entry.Requeue(++position, Now);

        var active = await persistence.ActiveSessionsAsync(eventId, Now, ct);
        var slots = Math.Max(0, Math.Clamp(options.Value.ConcurrentBuyersPerEvent, 1, 1000) - active);
        var waiting = await persistence.ListWaitingEntriesAsync(eventId, slots, ct);

        foreach (var entry in waiting)
        {
            if (await customers.GetByIdAsync(entry.CustomerId, tenant, ct) is not { IsActive: true })
                continue;

            if ((!drop.IsPublic || drop.RequiresRegistration) && !await persistence.IsEligibleAsync(eventId, entry.CustomerId, ct))
                continue;

            entry.Admit(Now);
            persistence.Add(QueueSession.Create(entry.Id, entry.CustomerId, OpaqueToken.Hash(OpaqueToken.Create()), 1, Now, Now.AddMinutes(15), Now));
        }

        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<bool>.Success(true);
    }

    private async Task<int> TailAsync(long eventId, CancellationToken ct)
    {
        return await persistence.TailAsync(eventId, ct) ?? 0;
    }

    private Task<bool> HasProductsAsync(long tenant, long eventId, CancellationToken ct)
    {
        return persistence.HasProductsAsync(eventId, tenant, ct);
    }
}
