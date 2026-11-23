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
public sealed class StorefrontWaitlistEntryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista as participações do cliente em listas de espera.
    /// </summary>
    [HttpGet("waitlist")]
    public async Task<IActionResult> Waitlist(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListMyDropWaitlistEntriesQuery(), ct));
    }
}
