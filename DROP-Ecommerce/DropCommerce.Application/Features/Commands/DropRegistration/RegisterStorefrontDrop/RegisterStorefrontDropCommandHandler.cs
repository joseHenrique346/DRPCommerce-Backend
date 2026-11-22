using DropCommerce.Domain.Interfaces;
using DropCommerce.Application.Security;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.StaticEntity;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class RegisterStorefrontDropCommandHandler(
    IDropStorefrontRepository persistence,
    IUnitOfWork unitOfWork,
    ICurrentDropActor actor,
    IDropOperationScope scope,
    TimeProvider clock)
    : IRequestHandler<RegisterStorefrontDropCommand, Result<DropRegistrationResponse>>
{
    public async Task<Result<DropRegistrationResponse>> Handle(RegisterStorefrontDropCommand command, CancellationToken ct)
    {
        long id = command.Id;

        var customerId = actor.CustomerId;
        var tenant = actor.EnterpriseId;
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop)
            throw new SecurityFailure(409, "DropContract.ModuleRequired", "O contrato atual não permite novas inscrições DROP.");

        using var operation = scope.Begin();
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockEventAsync(id, tenant, ct);
        var drop = await persistence.FindEventAsync(id, ct)
            ?? throw new SecurityFailure(404, "Drop.NotFound", "Evento indisponível.");
        var now = clock.GetUtcNow().UtcDateTime;

        if (!DropEventPublication.IsPublished(drop) || now < drop.RegistrationStartsAt || now > drop.RegistrationEndsAt ||
            !await persistence.HasEligibleProductAsync(id, ct))
            throw new SecurityFailure(409, "Drop.RegistrationClosed", "Inscrições fora do período permitido.");

        var registration = await persistence.FindRegistrationAsync(id, customerId, ct);

        if (registration is not null)
            return Result<DropRegistrationResponse>.Success(new DropRegistrationResponse(registration.Id, registration.DropRegistrationStatusId, registration.IsEligible));

        registration = DropRegistration.Create(id, customerId, DropRegistrationStatus.Pending.Id, false,
            "Aguardando avaliação de elegibilidade", now, null);
        persistence.Add(registration);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropRegistrationResponse>.Success(new DropRegistrationResponse(registration.Id, registration.DropRegistrationStatusId, registration.IsEligible));
    }
}
