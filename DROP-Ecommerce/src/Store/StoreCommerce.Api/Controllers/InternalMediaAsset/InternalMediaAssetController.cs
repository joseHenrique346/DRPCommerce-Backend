using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Interfaces;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/internal/media-assets")]
[AllowAnonymous]
public sealed class InternalMediaAssetController(IMediaAssetReader media, ITenantProvider tenant,
    StoreCommerce.Application.Media.MediaRetentionService retention) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var asset = await media.GetAsync(tenant.GetEnterpriseId(), id, ct);

        return asset is null ? NotFound() : Ok(Result<SharedCommerce.Application.Contracts.Appearance.MediaAssetReference>.Success(asset));
    }

    [HttpPost("{id:long}/retain-for-drop")]
    public async Task<IActionResult> Retain(long id, CancellationToken ct)
    {
        var asset = await retention.RetainAsync(tenant.GetEnterpriseId(), id, ct);

        return asset is null ? NotFound() : Ok(Result<SharedCommerce.Application.Contracts.Appearance.MediaAssetReference>.Success(asset));
    }
}
