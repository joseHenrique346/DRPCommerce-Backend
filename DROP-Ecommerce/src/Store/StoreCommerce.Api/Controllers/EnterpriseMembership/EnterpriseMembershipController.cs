using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[Route("api/enterprise-memberships")]
public class EnterpriseMembershipController(IMediator mediator)
    : MappedCrudController<EnterpriseMembership, EnterpriseMembershipResponse, CreateEnterpriseMembershipCommand, CreateListEnterpriseMembershipCommand, UpdateEnterpriseMembershipCommand, UpdateListEnterpriseMembershipCommand>(mediator)
{
    protected override EnterpriseMembershipResponse MapEntity(EnterpriseMembership entity) => new(entity.Id, entity.EnterpriseId, entity.UserId, entity.KindId, entity.IsActive, entity.JoinedAt);
    protected override CreateListEnterpriseMembershipCommand WrapCreateInRange(CreateEnterpriseMembershipCommand command) => new([command]);
    protected override UpdateListEnterpriseMembershipCommand WrapUpdateInRange(UpdateEnterpriseMembershipCommand command) => new([command]);
    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids) => new DeleteListEnterpriseMembershipCommand(ids);
    protected override IRequest<Result<List<EnterpriseMembership>>> GetAllQuery() => new GetAllEnterpriseMembershipQuery();
    protected override IRequest<Result<EnterpriseMembership>> GetByIdQuery(long id) => new GetByIdEnterpriseMembershipQuery(id);
    protected override IRequest<Result<List<EnterpriseMembership>>> GetListByListIdQuery(List<long> ids) => new GetListByListIdEnterpriseMembershipQuery(ids);
}
