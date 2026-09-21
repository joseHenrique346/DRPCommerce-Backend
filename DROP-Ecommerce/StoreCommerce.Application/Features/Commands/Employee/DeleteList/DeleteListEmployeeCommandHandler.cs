using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEmployeeCommandHandler(IRepository<Employee> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEmployeeCommand, Employee>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEmployeeCommand request) => request.ids;
}
