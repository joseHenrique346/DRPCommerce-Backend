using Commerce.Appearance.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Commerce.Appearance.Api;

public abstract class AppearancePublicControllerBase(IMediator mediator) : ControllerBase
{
    protected IMediator Mediator => mediator;

    protected IActionResult ManifestResult(AppearanceManifest manifest, bool immutable)
    {
        var tag = new EntityTagHeaderValue($"\"appearance-{manifest.RevisionId}-{manifest.Checksum}-{manifest.Source}\"");
        Response.Headers.ETag = tag.ToString();

        if (User.Identity?.IsAuthenticated != true)
            Response.Headers.CacheControl = immutable ? "public, max-age=86400, immutable" : "public, max-age=0, must-revalidate";

        if (Request.Headers.IfNoneMatch.Count == 0)
            return Ok(manifest);

        if (!EntityTagHeaderValue.TryParseList(Request.Headers.IfNoneMatch, out var requested))
            return BadRequest(new { code = "Appearance.InvalidETag", message = "Header If-None-Match inválido." });

        if (requested.Any(Tag => Tag.Tag == "*" || Tag.Compare(tag, useStrongComparison: false)))
            return StatusCode(304);

        return Ok(manifest);
    }
}
