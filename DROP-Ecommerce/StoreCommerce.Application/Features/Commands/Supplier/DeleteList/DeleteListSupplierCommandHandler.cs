using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListSupplierCommandHandler(IRepository<Supplier> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListSupplierCommand, Supplier>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListSupplierCommand request) => request.ids;
}
