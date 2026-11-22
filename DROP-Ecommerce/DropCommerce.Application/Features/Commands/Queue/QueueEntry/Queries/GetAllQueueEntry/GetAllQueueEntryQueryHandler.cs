using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetAllQueueEntryQueryHandler(IRepository<QueueEntry> repository)
    : BaseGetAllHandler<GetAllQueueEntryQuery, QueueEntry>(repository);
