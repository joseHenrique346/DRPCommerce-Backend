using DropCommerce.Application.Interfaces.Storefront;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListMyDropWaitlistEntriesQueryHandler(
    IDropStorefrontReader reader)
    : IRequestHandler<ListMyDropWaitlistEntriesQuery, List<DropWaitlistEntrySummary>>
{
    public async Task<List<DropWaitlistEntrySummary>> Handle(ListMyDropWaitlistEntriesQuery request, CancellationToken ct)
    {
        return await reader.ListMyDropWaitlistEntriesAsync(ct);
    }
}
