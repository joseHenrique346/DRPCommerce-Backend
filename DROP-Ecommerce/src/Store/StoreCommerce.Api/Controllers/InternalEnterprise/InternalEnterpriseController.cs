using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Result;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalEnterpriseController(StoreCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Consulta uma loja para integração interna.
    /// </summary>
    [HttpGet("enterprises/get-by-id/{id:long}")]
    public async Task<IActionResult> Enterprise(long id, CancellationToken ct)
    {
        var value = await db.Enterprises
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                EnterpriseId = x.Id,
                x.TradeName,
                x.LegalName,
                x.IsActive
            })
            .SingleOrDefaultAsync(ct);

        return value is null ? NotFound() : Ok(Result<object>.Success(value));
    }
}

