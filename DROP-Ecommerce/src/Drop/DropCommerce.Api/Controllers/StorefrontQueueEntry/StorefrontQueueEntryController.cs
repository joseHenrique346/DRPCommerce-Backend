using DropCommerce.Infrastructure.Data.Contracts;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontQueueEntryController(DropCommerceDbContext db, DropQueueService queue) : ControllerBase
{
    [HttpPost("events/{id:long}/queue")]
    public async Task<IActionResult> JoinQueue(long id, JoinDropQueueRequest request, CancellationToken ct)
    {
        return Ok(await queue.JoinAsync(id, request, ct));
    }

    [HttpGet("events/{id:long}/queue")]
    public async Task<IActionResult> Status(long id, CancellationToken ct)
    {
        return Ok(await queue.StatusAsync(id, ct));
    }

    [HttpPost("events/{id:long}/reservations")]
    public async Task<IActionResult> Reserve(long id, ReserveDropPurchase request, CancellationToken ct)
    {
        var reservation = await queue.ReserveAsync(id, request, ct);

        return Ok(new
        {
            reservation.Id, reservation.DropEventId, reservation.DropProductId, reservation.Quantity,
            reservation.TotalAmount, reservation.ExpiresAt,
            lines = reservation.Lines.Select(Line => new { Line.VariantId, Line.SKU, Line.ItemName, Line.Quantity, Line.UnitPrice, Line.OptionsJson })
        });
    }

    [HttpGet("queue")]
    public async Task<IActionResult> Queue(CancellationToken ct)
    {
        return Ok(await db.QueueEntries.AsNoTracking().Select(Entry => new
        {
            Entry.Id, Entry.DropEventId, Entry.Position, Entry.QueueEntryStatusId, Entry.EnteredAt
        }).ToListAsync(ct));
    }
}