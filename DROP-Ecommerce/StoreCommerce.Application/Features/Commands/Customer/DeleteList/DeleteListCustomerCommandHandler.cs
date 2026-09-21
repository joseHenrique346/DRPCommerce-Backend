using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListCustomerCommandHandler(IRepository<Customer> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListCustomerCommand, Customer>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListCustomerCommand request) => request.ids;
}
