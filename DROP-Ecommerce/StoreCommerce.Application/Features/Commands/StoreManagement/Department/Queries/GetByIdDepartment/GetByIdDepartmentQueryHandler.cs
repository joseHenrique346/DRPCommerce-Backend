using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdDepartmentQueryHandler(IRepository<Department> repository)
    : BaseGetByIdHandler<GetByIdDepartmentQuery, Department>(repository)
{
    protected override long GetById(GetByIdDepartmentQuery request) => request.id;
}
