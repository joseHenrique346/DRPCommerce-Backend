using Commerce.Appearance.Api;
using Commerce.Appearance.Application.Features;
using Commerce.Appearance.Contracts;
using DropCommerce.Application.Appearance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop/appearance")]
[AllowAnonymous]
[EnableRateLimiting("api")]
public sealed class StorefrontAppearanceController(IMediator mediator) : AppearancePublicControllerBase(mediator)
{
    /// <summary>
    /// Consulta a aparência publicada dos drops ou uma revisão específica.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Commerce.Appearance.Contracts.AppearanceManifest), 200)]
    [ProducesResponseType(304)]
    [HttpGet("revisions/{revisionId:long:min(1)}")]
    public async Task<IActionResult> Get(long? revisionId, CancellationToken ct)
    {
        var manifest = await Mediator.Send(new GetPublishedAppearanceQuery(DropAppearanceTargets.Default, revisionId), ct);

        return ManifestResult(manifest, revisionId.HasValue);
    }
}
