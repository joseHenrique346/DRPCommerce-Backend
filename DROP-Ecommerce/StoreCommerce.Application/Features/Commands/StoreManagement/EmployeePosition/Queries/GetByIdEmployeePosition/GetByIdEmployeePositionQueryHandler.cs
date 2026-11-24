using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEmployeePositionQueryHandler(IEmployeePositionRepository repository)
    : BaseGetByIdHandler<GetByIdEmployeePositionQuery, EmployeePosition>(repository)
{
    protected override long GetById(GetByIdEmployeePositionQuery request) => request.id;
}
