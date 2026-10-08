using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/membership-access-roles")]
public class MembershipAccessRoleController(IMediator mediator) : ControllerBase
{
    [HttpPost("add")]
    public async Task<IActionResult> AddAsync([FromBody] CreateMembershipAccessRoleCommand command, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command, cancellationToken));

    [HttpDelete("delete/{enterpriseMembershipId:long}/{accessRoleId:long}")]
    public async Task<IActionResult> DeleteAsync(long enterpriseMembershipId, long accessRoleId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteMembershipAccessRoleCommand(enterpriseMembershipId, accessRoleId), cancellationToken));

    [HttpGet("get-by-key/{enterpriseMembershipId:long}/{accessRoleId:long}")]
    public async Task<IActionResult> GetByKeyAsync(long enterpriseMembershipId, long accessRoleId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetByKeyMembershipAccessRoleQuery(enterpriseMembershipId, accessRoleId), cancellationToken));

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetAllMembershipAccessRoleQuery(), cancellationToken));
}
