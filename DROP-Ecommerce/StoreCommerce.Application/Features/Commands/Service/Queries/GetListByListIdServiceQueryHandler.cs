using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdServiceQueryHandler(IRepository<Service> repository)
    : BaseGetListByListIdHandler<GetListByListIdServiceQuery, Service>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdServiceQuery request) => request.listId;
}
