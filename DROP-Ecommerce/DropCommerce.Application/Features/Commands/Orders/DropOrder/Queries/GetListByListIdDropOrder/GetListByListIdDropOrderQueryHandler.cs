using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropOrderQueryHandler(IRepository<DropOrder> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropOrderQuery, DropOrder>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropOrderQuery request) => request.listId;
}
