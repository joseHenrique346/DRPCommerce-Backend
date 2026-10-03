using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/roles")]
public class RoleController : BaseController<Role, CreateRoleCommand, CreateListRoleCommand, UpdateRoleCommand, UpdateListRoleCommand>
{
    public RoleController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListRoleCommand WrapCreateInRange(CreateRoleCommand command)
    {
        return new CreateListRoleCommand(new List<CreateRoleCommand> { command });
    }

    protected override UpdateListRoleCommand WrapUpdateInRange(UpdateRoleCommand command)
    {
        return new UpdateListRoleCommand(new List<UpdateRoleCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListRoleCommand(ids);
    }

    protected override IRequest<Result<List<Role>>> GetAllQuery()
    {
        return new GetAllRoleQuery();
    }

    protected override IRequest<Result<Role>> GetByIdQuery(long id)
    {
        return new GetByIdRoleQuery(id);
    }

    protected override IRequest<Result<List<Role>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdRoleQuery(ids);
    }
}
