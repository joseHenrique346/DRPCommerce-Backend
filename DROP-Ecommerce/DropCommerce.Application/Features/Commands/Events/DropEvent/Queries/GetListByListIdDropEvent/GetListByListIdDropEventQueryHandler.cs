using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropEventQueryHandler(IRepository<DropEvent> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropEventQuery, DropEvent>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropEventQuery request) => request.listId;
}
