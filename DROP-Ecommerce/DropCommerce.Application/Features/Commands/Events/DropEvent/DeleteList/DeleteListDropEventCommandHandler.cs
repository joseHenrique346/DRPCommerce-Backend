using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropEventCommandHandler(IRepository<DropEvent> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropEventCommand, DropEvent>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropEventCommand request) => request.ids;
}
