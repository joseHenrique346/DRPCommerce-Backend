using Commerce.Appearance.Application.Features;
using Commerce.Appearance.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Commerce.Appearance.Api;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("api")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[RequestSizeLimit(1048576)]
public abstract class AppearanceThemeCatalogControllerBase(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registra as definições de temas publicadas pela plataforma.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Register(InputThemeCatalog input,
        [FromHeader(Name = "X-Service-Key")] string? serviceKey, CancellationToken ct)
    {
        await mediator.Send(new RegisterAppearanceThemesCommand(input), ct);

        return NoContent();
    }
}
