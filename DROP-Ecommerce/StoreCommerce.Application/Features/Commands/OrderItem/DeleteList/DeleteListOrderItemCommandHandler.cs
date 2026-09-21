using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListOrderItemCommandHandler(IRepository<OrderItem> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListOrderItemCommand, OrderItem>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListOrderItemCommand request) => request.ids;
}
