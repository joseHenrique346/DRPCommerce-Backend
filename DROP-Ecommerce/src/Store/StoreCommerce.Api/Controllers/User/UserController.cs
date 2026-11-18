using SharedCommerce.Application.Security;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;
using StoreCommerce.Api.Security;
using Microsoft.AspNetCore.RateLimiting;

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
    /// Informa impedimentos para excluir a própria conta global. A exclusão revalida as condições sob bloqueio.
    /// </summary>
    [HttpGet("me/deletion")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetDeletionEligibility(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetAccountDeletionEligibilityQuery(false), ct));
    }

    /// <summary>
    /// Apaga o perfil e encerra os acessos, mediante senha atual e ausência de lojas sob sua propriedade.
    /// </summary>
    [HttpDelete("me")]
    [CookieCsrf]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Deactivate([FromBody] DeleteAccountRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateUserCommand(request), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        Response.ClearDeletedAccountCookie(false);

        return NoContent();
    }
}
