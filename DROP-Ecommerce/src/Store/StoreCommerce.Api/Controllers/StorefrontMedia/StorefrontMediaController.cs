using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Application.Media;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/media")]
[AllowAnonymous]
[EnableRateLimiting("api")]
public sealed class StorefrontMediaController(IMediaAssetStore assets, ITenantProvider tenant, StoreCommerceDbContext db) : ControllerBase
{
    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> Get(Guid publicId, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var rights = await db.ReadAppearanceCapabilitiesAsync(enterpriseId, "store", ct);

        if (!rights.AllowsLogo)
            return NotFound();

        var asset = await assets.FindAsync(enterpriseId, publicId, ct);

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
