using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontProductController(StoreCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista os produtos disponíveis na loja.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 100000);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return Ok(await db.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.Price,
                x.Brand,
                x.ImageUrls,
                x.CategoryId
            })
            .ToListAsync(ct));
    }
}

