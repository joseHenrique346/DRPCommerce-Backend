using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdQueueEntryQueryHandler(IRepository<QueueEntry> repository)
    : BaseGetListByListIdHandler<GetListByListIdQueueEntryQuery, QueueEntry>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdQueueEntryQuery request) => request.listId;
}
