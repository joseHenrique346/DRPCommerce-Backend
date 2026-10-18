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
[Route("api/identity/invitations")]
[Authorize(Policy = SecurityNames.GlobalUser)]
[EnableRateLimiting("auth")]
public sealed class InvitationAcceptanceController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Aceita um convite vinculado ao e-mail do usuário.
    /// </summary>
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(InvitationAcceptanceRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new AcceptInvitationCommand(request.Token), ct));
    }
}

