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
public sealed class StorefrontDropRegistrationController(IMediator mediator) : ControllerBase
{
    [HttpPost("events/{id:long}/register")]
    public async Task<IActionResult> Register(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterStorefrontDropCommand(id), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpGet("registrations")]
    public async Task<IActionResult> Registrations(CancellationToken ct)
    {
        var result = await mediator.Send(new ListCustomerDropRegistrationsQuery(), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }
}
