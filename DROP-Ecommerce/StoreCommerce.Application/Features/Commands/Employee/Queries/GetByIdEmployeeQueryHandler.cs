using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEmployeeQueryHandler(IRepository<Employee> repository)
    : BaseGetByIdHandler<GetByIdEmployeeQuery, Employee>(repository)
{
    protected override long GetById(GetByIdEmployeeQuery request) => request.id;
}
