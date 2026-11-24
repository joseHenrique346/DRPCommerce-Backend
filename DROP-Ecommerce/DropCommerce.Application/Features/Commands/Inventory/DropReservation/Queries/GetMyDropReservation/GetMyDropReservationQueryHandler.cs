using DropCommerce.Application.Interfaces.Storefront;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class GetMyDropReservationQueryHandler(
    IDropStorefrontReader reader)
    : IRequestHandler<GetMyDropReservationQuery, DropReservationSummary?>
{
    public async Task<DropReservationSummary?> Handle(GetMyDropReservationQuery request, CancellationToken ct)
    {
        return await reader.GetMyDropReservationAsync(request.Id, ct);
    }
}
