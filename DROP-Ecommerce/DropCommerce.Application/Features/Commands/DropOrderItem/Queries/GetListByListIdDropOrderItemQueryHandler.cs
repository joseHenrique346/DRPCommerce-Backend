using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropOrderItemQueryHandler(IRepository<DropOrderItem> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropOrderItemQuery, DropOrderItem>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropOrderItemQuery request) => request.listId;
}
