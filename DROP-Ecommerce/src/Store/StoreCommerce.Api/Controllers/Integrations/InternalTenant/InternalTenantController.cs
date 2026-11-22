using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalTenantController(StoreCommerce.Application.Interfaces.StoreManagement.IEnterpriseDomainAccess domains) : ControllerBase
{
    /// <summary>
    /// Identifica a loja pelo domínio para integração interna.
    /// </summary>
    [HttpGet("tenants/resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string host, CancellationToken ct)
    {
        var normalized = HostName.Normalize(host);
        var tenantId = await domains.ResolveAsync(normalized, null, ct);

        return tenantId is null ? NotFound() : Ok(new { EnterpriseId = tenantId.Value });
    }
}
