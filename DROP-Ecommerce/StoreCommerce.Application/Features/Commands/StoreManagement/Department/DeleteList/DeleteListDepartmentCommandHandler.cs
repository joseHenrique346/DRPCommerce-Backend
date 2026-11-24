using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListDepartmentCommandHandler(IRepository<Department> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDepartmentCommand, Department>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDepartmentCommand request) => request.ids;
}
