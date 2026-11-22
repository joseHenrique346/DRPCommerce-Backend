using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropOrderItemQueryHandler(IRepository<DropOrderItem> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropOrderItemQuery, DropOrderItem>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropOrderItemQuery request) => request.listId;
}
