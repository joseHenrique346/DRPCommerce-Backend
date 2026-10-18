using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/access-roles")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class AccessRoleController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os perfis de acesso da loja.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListRolesQuery(), ct));
    }

    /// <summary>
    /// Cadastra um perfil de acesso na loja.
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> Create(AccessRoleRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new SaveRoleCommand(null, request), ct));
    }

    /// <summary>
    /// Atualiza o perfil de acesso e suas permissões.
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, AccessRoleRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new SaveRoleCommand(id, request), ct));
    }

    /// <summary>
    /// Exclui um perfil de acesso sem vínculos.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
