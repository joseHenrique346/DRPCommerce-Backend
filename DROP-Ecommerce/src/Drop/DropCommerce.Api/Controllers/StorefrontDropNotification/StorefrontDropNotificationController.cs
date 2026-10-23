using SharedCommerce.Application.Security;
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
public sealed class StorefrontDropNotificationController(DropCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista as notificações de drop do cliente.
    /// </summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        return Ok(await db.DropNotifications
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.Subject,
                x.Body,
                x.ScheduledAt
            })
            .ToListAsync(ct));
    }
}

