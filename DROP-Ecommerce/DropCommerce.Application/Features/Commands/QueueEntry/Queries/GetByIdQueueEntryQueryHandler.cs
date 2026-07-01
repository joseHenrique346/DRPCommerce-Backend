using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdQueueEntryQueryHandler(IRepository<QueueEntry> repository)
    : BaseGetByIdHandler<GetByIdQueueEntryQuery, QueueEntry>(repository)
{
    protected override long GetById(GetByIdQueueEntryQuery request) => request.id;
}
