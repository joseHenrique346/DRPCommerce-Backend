using MediatR;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Contracts;
using DropCommerce.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontQueueEntryController(IMediator mediator) : ControllerBase
{
    [HttpPost("events/{id:long}/queue")]
    public async Task<IActionResult> JoinQueue(long id, JoinDropQueueRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new JoinDropQueueCommand(id, request), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpGet("events/{id:long}/queue")]
    public async Task<IActionResult> Status(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDropQueueStatusQuery(id), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpPost("events/{id:long}/reservations")]
    public async Task<IActionResult> Reserve(long id, ReserveDropPurchase request, CancellationToken ct)
    {
        var result = await mediator.Send(new ReserveDropPurchaseCommand(id, request), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> Queue(CancellationToken ct)
    {
        var result = await mediator.Send(new ListDropQueueEntriesQuery(), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }
}
