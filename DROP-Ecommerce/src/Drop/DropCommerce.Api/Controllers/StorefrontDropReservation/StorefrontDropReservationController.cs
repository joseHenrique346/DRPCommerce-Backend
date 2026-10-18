using Commerce.Security;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropReservationController(DropCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista as reservas de drop do cliente.
    /// </summary>
    [HttpGet("reservations")]
    public async Task<IActionResult> Reservations(CancellationToken ct)
    {
        return Ok(await db.DropReservations
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.DropEventId,
                x.DropProductId,
                x.Quantity,
                x.TotalAmount,
                x.DropReservationStatusId,
                x.ExpiresAt
            })
            .ToListAsync(ct));
    }

    /// <summary>
    /// Consulta uma reserva de drop do cliente.
    /// </summary>
    [HttpGet("reservations/{id:long}")]
    public async Task<IActionResult> Reservation(long id, CancellationToken ct)
    {
        var value = await db.DropReservations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.DropEventId,
                x.DropProductId,
                x.Quantity,
                x.TotalAmount,
                x.DropReservationStatusId,
                x.ExpiresAt
            })
            .SingleOrDefaultAsync(ct);

        return value is null ? NotFound() : Ok(value);
    }
}

