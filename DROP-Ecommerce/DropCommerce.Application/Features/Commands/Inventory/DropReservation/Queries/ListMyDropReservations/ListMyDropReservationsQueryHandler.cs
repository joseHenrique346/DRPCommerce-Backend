using DropCommerce.Application.Interfaces.Storefront;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListMyDropReservationsQueryHandler(
    IDropStorefrontReader reader)
    : IRequestHandler<ListMyDropReservationsQuery, List<DropReservationSummary>>
{
    public async Task<List<DropReservationSummary>> Handle(ListMyDropReservationsQuery request, CancellationToken ct)
    {
        return await reader.ListMyDropReservationsAsync(ct);
    }
}
