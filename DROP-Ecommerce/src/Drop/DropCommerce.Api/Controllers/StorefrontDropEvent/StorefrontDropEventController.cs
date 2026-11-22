using MediatR;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Api.Extensions;
using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropEventController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("events/{id:long}")]
    public async Task<IActionResult> Event(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStorefrontDropEventQuery(id), ct);

        if (result.IsFailure)
            return this.ToActionResult(result);

        return result.Content is null ? NotFound() : Ok(result.Content);
    }

    [AllowAnonymous]
    [HttpGet("events")]
    public async Task<IActionResult> Events(CancellationToken ct)
    {
        var result = await mediator.Send(new ListStorefrontDropEventsQuery(), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }
}
