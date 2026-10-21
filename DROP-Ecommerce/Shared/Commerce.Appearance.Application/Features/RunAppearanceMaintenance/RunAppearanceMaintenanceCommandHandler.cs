using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class RunAppearanceMaintenanceCommandHandler(
    IAppearanceRepository repository,
    TimeProvider clock,
    IMediator mediator) : IRequestHandler<RunAppearanceMaintenanceCommand>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task Handle(RunAppearanceMaintenanceCommand request, CancellationToken ct)
    {
        var targets = await repository.GetMigrationTargetsAsync(Now, ct);

        foreach (var (tenant, target) in targets)
            await mediator.Send(new MigrateAppearanceCommand(tenant, target), ct);
    }
}
