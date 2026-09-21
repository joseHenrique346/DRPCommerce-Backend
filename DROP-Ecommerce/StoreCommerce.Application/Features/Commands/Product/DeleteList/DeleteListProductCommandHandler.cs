using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListProductCommand, Product>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListProductCommand request) => request.ids;
}
