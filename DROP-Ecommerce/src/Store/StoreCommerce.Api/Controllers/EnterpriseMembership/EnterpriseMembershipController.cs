using SharedCommerce.Application.Security;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprise-memberships")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class EnterpriseMembershipController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os membros da loja.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> Members(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListMembersQuery(), ct));
    }

    /// <summary>
    /// Atualiza o tipo e a ativação de um membro.
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, MembershipUpdateRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new UpdateMembershipCommand(id, request), ct));
    }

    /// <summary>
    /// Define os perfis de acesso de um membro.
    /// </summary>
    [HttpPut("{id:long}/access-roles")]
    public async Task<IActionResult> Assign(long id, MembershipRolesRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AssignRolesCommand(id, request), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
