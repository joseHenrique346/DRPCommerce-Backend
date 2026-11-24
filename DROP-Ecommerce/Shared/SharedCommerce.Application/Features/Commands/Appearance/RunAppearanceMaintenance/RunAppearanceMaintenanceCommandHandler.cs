using SharedCommerce.Application.Interfaces.Appearance;
using MediatR;

namespace SharedCommerce.Application.Features.Commands;

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
