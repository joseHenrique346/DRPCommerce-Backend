using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetStoreSettingsQueryHandler(StoreSettingsReader reader) : IRequestHandler<GetStoreSettingsQuery, Result<StoreSettings>>
{
    public async Task<Result<StoreSettings>> Handle(GetStoreSettingsQuery request, CancellationToken cancellationToken) =>
        Result<StoreSettings>.Success(await reader.GetCurrentAsync(cancellationToken));
}
