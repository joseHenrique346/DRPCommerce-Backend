using MediatR;
using DropCommerce.Application.Features.Commands;
using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropReservationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista as reservas de drop do cliente.
    /// </summary>
    [HttpGet("reservations")]
    public async Task<IActionResult> Reservations(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListMyDropReservationsQuery(), ct));
    }

    /// <summary>
    /// Consulta uma reserva de drop do cliente.
    /// </summary>
    [HttpGet("reservations/{id:long}")]
    public async Task<IActionResult> Reservation(long id, CancellationToken ct)
    {
        var value = await mediator.Send(new GetMyDropReservationQuery(id), ct);

        return value is null ? NotFound() : Ok(value);
    }
}
