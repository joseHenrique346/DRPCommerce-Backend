using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdOrderItemQueryHandler(IRepository<OrderItem> repository)
    : BaseGetListByListIdHandler<GetListByListIdOrderItemQuery, OrderItem>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdOrderItemQuery request) => request.listId;
}
