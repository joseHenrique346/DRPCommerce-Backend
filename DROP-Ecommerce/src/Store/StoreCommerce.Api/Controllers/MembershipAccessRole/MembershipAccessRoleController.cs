using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/membership-access-roles")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class MembershipAccessRoleController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os perfis atribuídos aos membros da loja.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListMembershipRolesQuery(), ct));
    }

    /// <summary>
    /// Atribui um perfil de acesso ao membro.
    /// </summary>
    [HttpPut("{membershipId:long}/{roleId:long}")]
    public async Task<IActionResult> Grant(long membershipId, long roleId, CancellationToken ct)
    {
        var result = await mediator.Send(new SetMembershipRoleCommand(membershipId, roleId, true), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }

    /// <summary>
    /// Remove um perfil de acesso do membro.
    /// </summary>
    [HttpDelete("{membershipId:long}/{roleId:long}")]
    public async Task<IActionResult> Revoke(long membershipId, long roleId, CancellationToken ct)
    {
        var result = await mediator.Send(new SetMembershipRoleCommand(membershipId, roleId, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
