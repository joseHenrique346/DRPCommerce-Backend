using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropEventQueryHandler(IRepository<DropEvent> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropEventQuery, DropEvent>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropEventQuery request) => request.listId;
}
