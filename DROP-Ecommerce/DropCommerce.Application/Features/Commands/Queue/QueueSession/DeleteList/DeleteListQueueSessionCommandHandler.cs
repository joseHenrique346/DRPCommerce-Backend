using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueSessionCommandHandler(IRepository<QueueSession> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListQueueSessionCommand, QueueSession>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListQueueSessionCommand request) => request.ids;
}
