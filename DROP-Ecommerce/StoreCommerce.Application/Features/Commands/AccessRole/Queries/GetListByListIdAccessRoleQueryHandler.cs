using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdAccessRoleQueryHandler(IAccessRoleRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdAccessRoleQuery, AccessRole>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdAccessRoleQuery request) => request.ids;
}
