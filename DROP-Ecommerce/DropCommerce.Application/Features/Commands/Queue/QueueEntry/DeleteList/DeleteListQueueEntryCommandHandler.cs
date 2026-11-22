using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueEntryCommandHandler(IRepository<QueueEntry> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListQueueEntryCommand, QueueEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListQueueEntryCommand request) => request.ids;
}
