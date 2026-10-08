using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/employees")]
public class EmployeeController : BaseController<Employee, CreateEmployeeCommand, CreateListEmployeeCommand, UpdateEmployeeCommand, UpdateListEmployeeCommand>
{
    public EmployeeController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListEmployeeCommand WrapCreateInRange(CreateEmployeeCommand command)
    {
        return new CreateListEmployeeCommand(new List<CreateEmployeeCommand> { command });
    }

    protected override UpdateListEmployeeCommand WrapUpdateInRange(UpdateEmployeeCommand command)
    {
        return new UpdateListEmployeeCommand(new List<UpdateEmployeeCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListEmployeeCommand(ids);
    }

    protected override IRequest<Result<List<Employee>>> GetAllQuery()
    {
        return new GetAllEmployeeQuery();
    }

    protected override IRequest<Result<Employee>> GetByIdQuery(long id)
    {
        return new GetByIdEmployeeQuery(id);
    }

    protected override IRequest<Result<List<Employee>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdEmployeeQuery(ids);
    }
}
