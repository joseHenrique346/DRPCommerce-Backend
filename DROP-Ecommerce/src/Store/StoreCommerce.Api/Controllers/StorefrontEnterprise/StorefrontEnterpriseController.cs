using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Security.Context;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontEnterpriseController(StoreCommerceDbContext db, IdentityContext actor) : ControllerBase
{
    /// <summary>
    /// Consulta os dados públicos da loja acessada.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("store")]
    public async Task<IActionResult> Store(CancellationToken ct)
    {
        return Ok(await db.Enterprises
            .Where(x => x.Id == actor.EnterpriseId)
            .Select(x => new
            {
                x.TradeName,
                x.Email,
                x.Phone
            })
            .SingleAsync(ct));
    }
}

