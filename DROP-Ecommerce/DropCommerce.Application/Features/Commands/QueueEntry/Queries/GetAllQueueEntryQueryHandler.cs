using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetAllQueueEntryQueryHandler(IRepository<QueueEntry> repository)
    : BaseGetAllHandler<GetAllQueueEntryQuery, QueueEntry>(repository);
