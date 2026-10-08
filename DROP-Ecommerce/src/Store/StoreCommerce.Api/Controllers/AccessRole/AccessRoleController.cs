using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[Route("api/access-roles")]
public class AccessRoleController(IMediator mediator)
    : MappedCrudController<AccessRole, AccessRoleResponse, CreateAccessRoleCommand, CreateListAccessRoleCommand, UpdateAccessRoleCommand, UpdateListAccessRoleCommand>(mediator)
{
    protected override AccessRoleResponse MapEntity(AccessRole entity) => new(entity.Id, entity.EnterpriseId, entity.Name, entity.NormalizedName, entity.Description, entity.IsActive);
    protected override CreateListAccessRoleCommand WrapCreateInRange(CreateAccessRoleCommand command) => new([command]);
    protected override UpdateListAccessRoleCommand WrapUpdateInRange(UpdateAccessRoleCommand command) => new([command]);
    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids) => new DeleteListAccessRoleCommand(ids);
    protected override IRequest<Result<List<AccessRole>>> GetAllQuery() => new GetAllAccessRoleQuery();
    protected override IRequest<Result<AccessRole>> GetByIdQuery(long id) => new GetByIdAccessRoleQuery(id);
    protected override IRequest<Result<List<AccessRole>>> GetListByListIdQuery(List<long> ids) => new GetListByListIdAccessRoleQuery(ids);
}
