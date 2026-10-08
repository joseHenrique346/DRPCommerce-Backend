using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/access-role-permissions")]
public class AccessRolePermissionController(IMediator mediator) : ControllerBase
{
    [HttpPost("add")]
    public async Task<IActionResult> AddAsync([FromBody] CreateAccessRolePermissionCommand command, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command, cancellationToken));

    [HttpDelete("delete/{accessRoleId:long}/{permissionId:long}")]
    public async Task<IActionResult> DeleteAsync(long accessRoleId, long permissionId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteAccessRolePermissionCommand(accessRoleId, permissionId), cancellationToken));

    [HttpGet("get-by-key/{accessRoleId:long}/{permissionId:long}")]
    public async Task<IActionResult> GetByKeyAsync(long accessRoleId, long permissionId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetByKeyAccessRolePermissionQuery(accessRoleId, permissionId), cancellationToken));

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetAllAccessRolePermissionQuery(), cancellationToken));
}
