using DropCommerce.Application.Interfaces.Storefront;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class GetMyDropOrderQueryHandler(
    IDropStorefrontReader reader)
    : IRequestHandler<GetMyDropOrderQuery, DropOrderDetails?>
{
    public async Task<DropOrderDetails?> Handle(GetMyDropOrderQuery request, CancellationToken ct)
    {
        return await reader.GetMyDropOrderAsync(request.Id, ct);
    }
}
