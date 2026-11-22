using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderCommandHandler(IRepository<DropOrder> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropOrderCommand, DropOrder>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropOrderCommand request) => request.ids;
}
