using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEmployeePositionQueryHandler(IEmployeePositionRepository repository)
    : BaseGetByIdHandler<GetByIdEmployeePositionQuery, EmployeePosition>(repository)
{
    protected override long GetById(GetByIdEmployeePositionQuery request) => request.id;
}
