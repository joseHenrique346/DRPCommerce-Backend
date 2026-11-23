using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalTenantController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Identifica a loja pelo slug para integração interna.
    /// </summary>
    [HttpGet("tenants/resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string slug, CancellationToken ct)
    {
        var tenantId = await sender.Send(new ResolveStorefrontTenantQuery(slug), ct);

        return tenantId is null ? NotFound() : Ok(new { EnterpriseId = tenantId.Value });
    }
}
