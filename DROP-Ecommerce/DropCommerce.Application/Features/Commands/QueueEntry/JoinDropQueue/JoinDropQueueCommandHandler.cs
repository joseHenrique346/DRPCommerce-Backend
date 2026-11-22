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

public sealed class JoinDropQueueCommandHandler(
    IDropQueueRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    IStoreCustomerReader customers,
    ICurrentDropActor actor,
    IOptions<DropQueueOptions> options,
    TimeProvider clock,
    StoreIntegrationScope integration,
    IMediator mediator)
    : IRequestHandler<JoinDropQueueCommand, Result<DropQueueStatusResponse>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    private long Customer => actor.CustomerId;

    public async Task<Result<DropQueueStatusResponse>> Handle(JoinDropQueueCommand command, CancellationToken ct)
    {
        long eventId = command.EventId;
        JoinDropQueueRequest request = command.Input;

        var tenant = actor.EnterpriseId;
        var customer = Customer;
        using var storeOperation = integration.Begin(tenant);
        var hash = DropPurchaseIdentity.Create(options.Value.PurchaseIdentityKey, tenant, eventId, request.CPF);
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop || await customers.GetByIdAsync(customer, tenant, ct) is not { IsActive: true })
            throw Failure("DropPurchase.Unavailable", "O módulo ou o comprador não está disponível.");

        using (writes.Begin("sales", tenant))
        {
            await using var tx = await persistence.BeginTransactionAsync(ct);
            await persistence.LockCommercialTenantAsync(tenant, ct);
            var drop = await persistence.FindEventAsync(eventId, ct)
                ?? throw new SecurityFailure(404, "Drop.NotFound", "Evento não encontrado.");
            var registered = await persistence.IsRegisteredAsync(eventId, customer, ct);

            if (!DropEventPublication.IsPublished(drop) || Now < drop.QueueOpensAt || Now >= drop.DropEndsAt ||
                (!drop.IsPublic || drop.RequiresRegistration) && !registered || !await HasProductsAsync(tenant, eventId, ct))
                throw Failure("Drop.QueueClosed", "O evento não admite novas entradas.");

            var entry = await persistence.FindEntryAsync(eventId, customer, ct);
            var position = await TailAsync(eventId, ct) + 1;

            if (entry is null)
            {
                var agent = actor.UserAgent;
                agent = string.IsNullOrWhiteSpace(agent) ? "unknown" : agent[..Math.Min(500, agent.Length)];
                entry = QueueEntry.Create(eventId, customer, OpaqueToken.Hash(OpaqueToken.Create()), position, 1,
                    "not-collected", actor.IpAddress ?? "0.0.0.0", agent,
                    Now, null, null, null);
                persistence.Add(entry);
            }
            else if (entry.QueueEntryStatusId is 5 or 6)
            {
                entry.Requeue(position, Now);
            }

            if (entry.PurchaseIdentityHash is not null && entry.PurchaseIdentityHash != hash)
                throw Failure("DropPurchase.IdentityConflict", "A identidade de compra não pode mudar dentro do evento.");

            entry.BindPurchaseIdentity(hash);
            await unitOfWork.CommitAsync(ct);
            await tx.CommitAsync(ct);
        }

        persistence.ClearTracking();
        await mediator.SendRequiredAsync(new AdmitDropQueueCommand(tenant, eventId), ct);

        return Result<DropQueueStatusResponse>.Success(await mediator.SendRequiredAsync(new GetDropQueueStatusQuery(eventId), ct));
    }

    private async Task<int> TailAsync(long eventId, CancellationToken ct)
    {
        return await persistence.TailAsync(eventId, ct) ?? 0;
    }

    private Task<bool> HasProductsAsync(long tenant, long eventId, CancellationToken ct)
    {
        return persistence.HasProductsAsync(eventId, tenant, ct);
    }

    private static SecurityFailure Failure(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }
}
