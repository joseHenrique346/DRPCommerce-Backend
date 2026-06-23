using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderItemCommandHandler(IRepository<DropOrderItem> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropOrderItemCommand, DropOrderItem>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropOrderItemCommand request) => request.ids;
}
