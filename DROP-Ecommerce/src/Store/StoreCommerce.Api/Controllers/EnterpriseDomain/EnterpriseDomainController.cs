using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[Route("api/enterprise-domains")]
public class EnterpriseDomainController(IMediator mediator)
    : MappedCrudController<EnterpriseDomain, EnterpriseDomainResponse, CreateEnterpriseDomainCommand, CreateListEnterpriseDomainCommand, UpdateEnterpriseDomainCommand, UpdateListEnterpriseDomainCommand>(mediator)
{
    protected override EnterpriseDomainResponse MapEntity(EnterpriseDomain entity) => new(entity.Id, entity.EnterpriseId, entity.Host, entity.NormalizedHost, entity.IsPrimary, entity.IsVerified, entity.IsActive);
    protected override CreateListEnterpriseDomainCommand WrapCreateInRange(CreateEnterpriseDomainCommand command) => new([command]);
    protected override UpdateListEnterpriseDomainCommand WrapUpdateInRange(UpdateEnterpriseDomainCommand command) => new([command]);
    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids) => new DeleteListEnterpriseDomainCommand(ids);
    protected override IRequest<Result<List<EnterpriseDomain>>> GetAllQuery() => new GetAllEnterpriseDomainQuery();
    protected override IRequest<Result<EnterpriseDomain>> GetByIdQuery(long id) => new GetByIdEnterpriseDomainQuery(id);
    protected override IRequest<Result<List<EnterpriseDomain>>> GetListByListIdQuery(List<long> ids) => new GetListByListIdEnterpriseDomainQuery(ids);
}
