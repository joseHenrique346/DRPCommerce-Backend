using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListOrderCommandHandler(IRepository<Order> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListOrderCommand, Order>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListOrderCommand request) => request.ids;
}
