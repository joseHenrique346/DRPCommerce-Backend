using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdAccessRoleQueryHandler(IAccessRoleRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdAccessRoleQuery, AccessRole>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdAccessRoleQuery request) => request.ids;
}
