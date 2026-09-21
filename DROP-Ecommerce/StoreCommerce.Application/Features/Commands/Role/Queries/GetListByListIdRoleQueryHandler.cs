using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdRoleQueryHandler(IRepository<Role> repository)
    : BaseGetListByListIdHandler<GetListByListIdRoleQuery, Role>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdRoleQuery request) => request.listId;
}
