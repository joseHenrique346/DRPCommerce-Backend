using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[Route("api/employee-positions")]
public class EmployeePositionController(IMediator mediator)
    : MappedCrudController<EmployeePosition, EmployeePositionResponse, CreateEmployeePositionCommand, CreateListEmployeePositionCommand, UpdateEmployeePositionCommand, UpdateListEmployeePositionCommand>(mediator)
{
    protected override EmployeePositionResponse MapEntity(EmployeePosition entity) => new(entity.Id, entity.EnterpriseId, entity.Name, entity.NormalizedName, entity.Description, entity.IsActive);
    protected override CreateListEmployeePositionCommand WrapCreateInRange(CreateEmployeePositionCommand command) => new([command]);
    protected override UpdateListEmployeePositionCommand WrapUpdateInRange(UpdateEmployeePositionCommand command) => new([command]);
    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids) => new DeleteListEmployeePositionCommand(ids);
    protected override IRequest<Result<List<EmployeePosition>>> GetAllQuery() => new GetAllEmployeePositionQuery();
    protected override IRequest<Result<EmployeePosition>> GetByIdQuery(long id) => new GetByIdEmployeePositionQuery(id);
    protected override IRequest<Result<List<EmployeePosition>>> GetListByListIdQuery(List<long> ids) => new GetListByListIdEmployeePositionQuery(ids);
}
