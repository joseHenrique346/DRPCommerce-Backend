using DropCommerce.Application.Interfaces.Catalog;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ApplyDropCommercialRightsCommandHandler(
    IDropCatalogRepository persistence)
    : IRequestHandler<ApplyDropCommercialRightsCommand>
{
    public async Task Handle(ApplyDropCommercialRightsCommand command, CancellationToken ct)
    {
        await persistence.ApplyCommercialRightsAsync(command.Rights, ct);
    }
}
