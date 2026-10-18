using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalTenantController(StoreCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Identifica a loja pelo domínio para integração interna.
    /// </summary>
    [HttpGet("tenants/resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string host, CancellationToken ct)
    {
        var normalized = HostName.Normalize(host);
        var domain = await db.EnterpriseDomains
            .IgnoreQueryFilters()
            .Where(x => x.NormalizedHost == normalized && x.IsVerified && x.IsActive && x.Enterprise.IsActive)
            .Select(x => new
            {
                x.EnterpriseId
            })
            .SingleOrDefaultAsync(ct);

        return domain is null ? NotFound() : Ok(domain);
    }
}

