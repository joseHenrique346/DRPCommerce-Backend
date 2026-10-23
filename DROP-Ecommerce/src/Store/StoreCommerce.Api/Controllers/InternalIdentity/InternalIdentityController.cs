using SharedCommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;
using StoreCommerce.Security.Services;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalIdentityController(JwtSessionService sessions) : ControllerBase
{
    /// <summary>
    /// Valida o token e o estado da sessão para integração interna.
    /// </summary>
    [HttpPost("identity/validate")]
    public async Task<IActionResult> Validate(SessionValidationRequest request, CancellationToken ct)
    {
        try
        {
            var principal = sessions.ValidateSignature(request.AccessToken);

            return Ok(new SessionValidationResponse(await sessions.ValidateAsync(principal, request.Host, ct)));
        }
        catch (SecurityFailure)
        {
            return Ok(new SessionValidationResponse(false));
        }
    }
}

