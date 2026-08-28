using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueSessionCommandHandler(IRepository<QueueSession> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListQueueSessionCommand, QueueSession>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListQueueSessionCommand request) => request.ids;
}
