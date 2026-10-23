using MediatR;
using SharedCommerce.Application.Interfaces;

namespace SharedCommerce.Application.Features.Commands;

public sealed class RetryAppearanceMigrationCommandHandler(ISender sender, IAppearanceContext context)
    : IRequestHandler<RetryAppearanceMigrationCommand>
{
    public Task Handle(RetryAppearanceMigrationCommand request, CancellationToken ct)
    {
        context.RequireAdministrator();

        return sender.Send(new MigrateAppearanceCommand(context.EnterpriseId, request.Target), ct);
    }
}
