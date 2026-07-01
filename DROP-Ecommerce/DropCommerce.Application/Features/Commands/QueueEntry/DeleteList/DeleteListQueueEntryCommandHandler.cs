using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueEntryCommandHandler(IRepository<QueueEntry> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListQueueEntryCommand, QueueEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListQueueEntryCommand request) => request.ids;
}
