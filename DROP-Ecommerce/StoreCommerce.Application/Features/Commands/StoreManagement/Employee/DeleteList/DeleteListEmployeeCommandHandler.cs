using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEmployeeCommandHandler(IRepository<Employee> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEmployeeCommand, Employee>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEmployeeCommand request) => request.ids;
}
