using MediatR;
using DropCommerce.Application.Features.Commands;
using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/{storeSlug}/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropNotificationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista as notificações de drop do cliente.
    /// </summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListMyDropNotificationsQuery(), ct));
    }
}
