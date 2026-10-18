using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = SecurityNames.GlobalUser)]
public sealed class UserController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta o perfil do usuário autenticado.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetUserQuery(), ct));
    }

    /// <summary>
    /// Atualiza o nome do usuário autenticado.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateUserProfileRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new UpdateUserProfileCommand(request), ct));
    }

    /// <summary>
    /// Desativa a conta do usuário sem lojas sob sua propriedade.
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> Deactivate(CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateUserCommand(), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
