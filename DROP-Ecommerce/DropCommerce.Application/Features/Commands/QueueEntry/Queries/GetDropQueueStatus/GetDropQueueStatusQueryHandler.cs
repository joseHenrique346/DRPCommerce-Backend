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

public sealed class GetDropQueueStatusQueryHandler(
    IDropQueueRepository persistence,
    ICurrentDropActor actor,
    TimeProvider clock)
    : IRequestHandler<GetDropQueueStatusQuery, Result<DropQueueStatusResponse>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    private long Customer => actor.CustomerId;

    public async Task<Result<DropQueueStatusResponse>> Handle(GetDropQueueStatusQuery command, CancellationToken ct)
    {
        long eventId = command.EventId;

        var customer = Customer;
        var tenant = actor.EnterpriseId;
        var entry = await persistence.FindCustomerEntryAsync(eventId, customer, tenant, ct)
            ?? throw new SecurityFailure(404, "Drop.QueueNotFound", "Entrada não encontrada.");
        var session = await persistence.FindActiveSessionAsync(entry.Id, customer, Now, ct);

        return Result<DropQueueStatusResponse>.Success(new DropQueueStatusResponse(entry.Id, entry.Position, entry.QueueEntryStatusId, session));
    }
}
