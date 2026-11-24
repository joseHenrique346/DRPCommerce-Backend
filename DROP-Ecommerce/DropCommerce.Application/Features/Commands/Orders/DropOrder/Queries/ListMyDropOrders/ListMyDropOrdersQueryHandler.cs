using DropCommerce.Application.Interfaces.Storefront;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListMyDropOrdersQueryHandler(
    IDropStorefrontReader reader)
    : IRequestHandler<ListMyDropOrdersQuery, List<DropOrderSummary>>
{
    public async Task<List<DropOrderSummary>> Handle(ListMyDropOrdersQuery request, CancellationToken ct)
    {
        return await reader.ListMyDropOrdersAsync(ct);
    }
}
