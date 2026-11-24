using DropCommerce.Application.Interfaces.Storefront;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListMyDropNotificationsQueryHandler(
    IDropStorefrontReader reader)
    : IRequestHandler<ListMyDropNotificationsQuery, List<DropNotificationSummary>>
{
    public async Task<List<DropNotificationSummary>> Handle(ListMyDropNotificationsQuery request, CancellationToken ct)
    {
        return await reader.ListMyDropNotificationsAsync(ct);
    }
}
