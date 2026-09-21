using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdOrderQueryHandler(IRepository<Order> repository)
    : BaseGetListByListIdHandler<GetListByListIdOrderQuery, Order>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdOrderQuery request) => request.listId;
}
