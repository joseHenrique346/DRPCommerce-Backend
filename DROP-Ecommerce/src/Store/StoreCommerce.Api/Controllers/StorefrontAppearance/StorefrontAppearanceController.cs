using SharedCommerce.Api.Controllers;
using SharedCommerce.Application.Features.Commands;
using StoreCommerce.Application.Appearance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/appearance")]
[AllowAnonymous]
[EnableRateLimiting("api")]
public sealed class StorefrontAppearanceController(IMediator mediator) : AppearancePublicControllerBase(mediator)
{
    /// <summary>
    /// Consulta a aparência publicada da vitrine ou uma revisão específica.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SharedCommerce.Application.Contracts.Appearance.AppearanceManifest), 200)]
    [ProducesResponseType(304)]
    [HttpGet("revisions/{revisionId:long:min(1)}")]
    public async Task<IActionResult> Get(long? revisionId, CancellationToken ct)
    {
        var manifest = await Mediator.Send(new GetPublishedAppearanceQuery(StoreAppearanceTargets.Default, revisionId), ct);

        return ManifestResult(manifest, revisionId.HasValue);
    }
}
