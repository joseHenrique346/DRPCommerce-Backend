using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/access-role-permissions")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class AccessRolePermissionController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista as permissões dos perfis da loja.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListRolePermissionsQuery(), ct));
    }

    /// <summary>
    /// Adiciona uma permissão ao perfil de acesso.
    /// </summary>
    [HttpPut("{roleId:long}/{permissionId:long}")]
    public async Task<IActionResult> Grant(long roleId, long permissionId, CancellationToken ct)
    {
        var result = await mediator.Send(new SetRolePermissionCommand(roleId, permissionId, true), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }

    /// <summary>
    /// Remove uma permissão do perfil de acesso.
    /// </summary>
    [HttpDelete("{roleId:long}/{permissionId:long}")]
    public async Task<IActionResult> Revoke(long roleId, long permissionId, CancellationToken ct)
    {
        var result = await mediator.Send(new SetRolePermissionCommand(roleId, permissionId, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
