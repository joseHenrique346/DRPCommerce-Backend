using MediatR;
using StoreCommerce.Application.Features.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/{storeSlug}/media")]
[AllowAnonymous]
[EnableRateLimiting("api")]
public sealed class StorefrontMediaController(IMediator mediator) : ControllerBase
{
    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> Get(Guid publicId, CancellationToken ct)
    {
        var asset = await mediator.Send(new FindStorefrontMediaAssetQuery(publicId), ct);

        if (asset is null)
            return NotFound();

        var etag = "\"" + asset.ContentHash + "\"";
        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'";

        if (Request.Headers.IfNoneMatch.Any(Value => Value == etag))
            return StatusCode(StatusCodes.Status304NotModified);

        return File(asset.Content, "image/png");
    }
}
