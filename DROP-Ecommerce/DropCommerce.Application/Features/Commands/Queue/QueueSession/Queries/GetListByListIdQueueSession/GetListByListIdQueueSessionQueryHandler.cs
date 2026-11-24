using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdQueueSessionQueryHandler(IRepository<QueueSession> repository)
    : BaseGetListByListIdHandler<GetListByListIdQueueSessionQuery, QueueSession>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdQueueSessionQuery request) => request.listId;
}
