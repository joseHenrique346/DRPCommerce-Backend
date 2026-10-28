using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SharedCommerce.Application.Security;

namespace StoreCommerce.Api.Controllers;

[Route("api/employees")]
public class EmployeeController : MappedCrudController<Employee, EmployeeResponse, CreateEmployeeCommand, CreateListEmployeeCommand, UpdateEmployeeCommand, UpdateListEmployeeCommand>
{
    public EmployeeController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Desativa o funcionário e seu vínculo com a loja, invalidando o acesso administrativo.
    /// </summary>
    [HttpPut("{id:long}/deactivate")]
    [Authorize(Policy = SecurityNames.PrimaryOwner)]
    public async Task<IActionResult> DeactivateAsync(long id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeactivateEmployeeCommand(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }

    protected override EmployeeResponse MapEntity(Employee entity) => new(entity.Id, entity.EnterpriseId, entity.EnterpriseMembershipId, entity.FullName, entity.PositionId, entity.DepartmentId, entity.IsActive, entity.HiredAt);

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
