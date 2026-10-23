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
public sealed class StorefrontWaitlistEntryController(DropCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista as participações do cliente em listas de espera.
    /// </summary>
    [HttpGet("waitlist")]
    public async Task<IActionResult> Waitlist(CancellationToken ct)
    {
        return Ok(await db.WaitlistEntries
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.DropEventId,
                x.DropProductId,
                x.Position,
                x.WaitlistEntryStatusId
            })
            .ToListAsync(ct));
    }
}

