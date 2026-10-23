using SharedCommerce.Api.Controllers;
using SharedCommerce.Application.Features.Commands;
using DropCommerce.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop/events/{slug}/appearance")]
[AllowAnonymous]
[EnableRateLimiting("api")]
public sealed class StorefrontDropAppearanceController(IMediator mediator) : AppearancePublicControllerBase(mediator)
{
    /// <summary>
    /// Consulta a aparência publicada do evento de drop ou uma revisão específica.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SharedCommerce.Application.Contracts.Appearance.AppearanceManifest), 200)]
    [ProducesResponseType(304)]
    [HttpGet("revisions/{revisionId:long:min(1)}")]
    public async Task<IActionResult> Get(string slug, long? revisionId, CancellationToken ct)
    {
        var target = await Mediator.Send(new ResolveAppearanceEventQuery(slug), ct);
        var manifest = await Mediator.Send(new GetPublishedAppearanceQuery(target, revisionId), ct);

        return ManifestResult(manifest, revisionId.HasValue);
    }
}
