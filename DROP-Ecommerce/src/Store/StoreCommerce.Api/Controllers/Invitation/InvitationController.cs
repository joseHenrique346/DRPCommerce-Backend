using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class InvitationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Cria um convite para entrar na loja.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Invite(InvitationRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new InviteCommand(request), ct));
    }

    /// <summary>
    /// Lista os convites da loja sem expor seus tokens.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListInvitationsQuery(), ct));
    }

    /// <summary>
    /// Revoga um convite da loja.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Revoke(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokeInvitationCommand(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}

