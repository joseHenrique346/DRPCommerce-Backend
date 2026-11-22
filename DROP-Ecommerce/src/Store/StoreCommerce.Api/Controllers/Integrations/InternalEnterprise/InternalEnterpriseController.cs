using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Result;
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

    /// <summary>
    /// Consulta direitos atuais de aparência para um módulo autenticado da própria loja.
    /// </summary>
    [HttpGet("enterprises/{id:long}/appearance-rights/{channel}")]
    public async Task<IActionResult> AppearanceRights(long id, string channel, CancellationToken ct)
    {
        if (HttpContext.Items[SharedCommerce.Application.Security.SecurityNames.TrustedServiceTenant] is not long tenant || tenant != id)
            return NotFound();

        var rights = await db.ReadAppearanceCapabilitiesAsync(id, channel, ct);

        return Ok(Result<SharedCommerce.Application.Contracts.Appearance.AppearanceCapabilities>.Success(rights));
    }
}
