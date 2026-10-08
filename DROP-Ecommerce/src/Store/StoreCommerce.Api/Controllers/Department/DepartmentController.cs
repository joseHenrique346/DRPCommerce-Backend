using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/departments")]
public class DepartmentController : MappedCrudController<Department, DepartmentResponse, CreateDepartmentCommand, CreateListDepartmentCommand, UpdateDepartmentCommand, UpdateListDepartmentCommand>
{
    public DepartmentController(IMediator mediator) : base(mediator)
    {
    }

    protected override DepartmentResponse MapEntity(Department entity) => new(entity.Id, entity.EnterpriseId, entity.Name, entity.Description, entity.IsActive);

    protected override CreateListDepartmentCommand WrapCreateInRange(CreateDepartmentCommand command)
    {
        return new CreateListDepartmentCommand(new List<CreateDepartmentCommand> { command });
    }

    protected override UpdateListDepartmentCommand WrapUpdateInRange(UpdateDepartmentCommand command)
    {
        return new UpdateListDepartmentCommand(new List<UpdateDepartmentCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDepartmentCommand(ids);
    }

    protected override IRequest<Result<List<Department>>> GetAllQuery()
    {
        return new GetAllDepartmentQuery();
    }

    protected override IRequest<Result<Department>> GetByIdQuery(long id)
    {
        return new GetByIdDepartmentQuery(id);
    }

    protected override IRequest<Result<List<Department>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDepartmentQuery(ids);
    }
}
