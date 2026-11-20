using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedCommerce.Application.Security;
using StoreCommerce.Application.Media;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/media-assets")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("api")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class MediaAssetController(MediaAssetService media) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return Ok(await media.ListAsync(ct));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PngLogoValidator.MaximumBytes + 65536)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.Length is < 1 or > PngLogoValidator.MaximumBytes)
            return Problem(statusCode: 413, title: "O logo deve ter no máximo 1 MiB.");

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        var asset = await media.UploadAsync(buffer.ToArray(), ct);

        return CreatedAtAction(nameof(GetContent), new { id = asset.Id }, asset);
    }

    [HttpGet("{id:long}/content")]
    public async Task<IActionResult> GetContent(long id, CancellationToken ct)
    {
        var asset = await media.FindAsync(id, ct);

        if (asset is null)
            return NotFound();

        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'";

        return File(asset.Content, "image/png");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await media.DeleteAsync(id, ct);

        return NoContent();
    }
}
