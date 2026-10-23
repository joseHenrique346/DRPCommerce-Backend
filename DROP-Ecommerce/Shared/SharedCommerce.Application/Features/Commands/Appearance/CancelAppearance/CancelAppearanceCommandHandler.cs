using MediatR;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Application.Features.Commands;

public sealed class CancelAppearanceCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    TimeProvider clock) : IRequestHandler<CancelAppearanceCommand>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task Handle(CancelAppearanceCommand request, CancellationToken ct)
    {
        var target = request.Target;
        var sessionId = request.SessionId;
        var version = request.Version;
        var force = request.Force;

        context.RequireAdministrator();
        await using var transaction = await repository.BeginTransactionAsync(context.EnterpriseId, ct);
        var appearance = await repository.GetAsync(context.EnterpriseId, target, ct) ?? throw NotFound();
        var session = await repository.GetSessionAsync(context.EnterpriseId, sessionId, ct) ?? throw NotFound();

        if (session.AppearanceId != appearance.Id)
            throw NotFound();

        if (!force)
            session.RequireEditable(context.UserId, version, Now);
        else if (session.Version != version || !session.IsOpen)
            throw new AppearanceFailure(412, "Appearance.VersionConflict", "A sessão mudou. Recarregue antes de liberá-la.");

        session.Close(force ? AppearanceEditSessionStatus.Released : AppearanceEditSessionStatus.Cancelled);
        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private static AppearanceFailure NotFound()
    {
        return new AppearanceFailure(404, "Appearance.NotFound", "Aparência ou revisão indisponível.");
    }
}
