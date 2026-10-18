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
public sealed class StorefrontDropEventController(DropCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista os eventos públicos de drop da loja.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("events")]
    public async Task<IActionResult> Events(CancellationToken ct)
    {
        return Ok(await db.DropEvents
            .AsNoTracking()
            .Where(x => x.IsPublic)
            .OrderBy(x => x.DropStartsAt)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.Price,
                x.CoverImageUrl,
                x.DropStartsAt,
                x.DropEndsAt,
                x.RequiresRegistration
            })
            .ToListAsync(ct));
    }
}

