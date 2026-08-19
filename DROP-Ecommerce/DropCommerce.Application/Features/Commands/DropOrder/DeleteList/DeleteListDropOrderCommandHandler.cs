using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderCommandHandler(IRepository<DropOrder> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropOrderCommand, DropOrder>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropOrderCommand request) => request.ids;
}
