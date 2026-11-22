using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.StoreManagement;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetStoreActivityQueryHandler(IStoreActivityReader reader, ICurrentIdentity actor)
    : CatalogRequestHandler<GetStoreActivityQuery, PagedResponse<StoreActivityResponse>>
{
    protected override Task<PagedResponse<StoreActivityResponse>> ExecuteAsync(GetStoreActivityQuery request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.StoreHistoryView);

        return reader.ReadAsync(actor.EnterpriseId, request.Request, ct);
    }
}
