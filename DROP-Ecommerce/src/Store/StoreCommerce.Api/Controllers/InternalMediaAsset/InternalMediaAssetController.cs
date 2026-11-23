using MediatR;
using StoreCommerce.Application.Features.Commands;
using SharedCommerce.Application.Interfaces.Media;
using SharedCommerce.Domain.Interfaces.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/internal/media-assets")]
[AllowAnonymous]
public sealed class InternalMediaAssetController(IMediaAssetReader media, ITenantProvider tenant,
    ISender sender) : ControllerBase
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
        var asset = await sender.Send(new RetainDropMediaAssetCommand(tenant.GetEnterpriseId(), id), ct);

        return asset is null ? NotFound() : Ok(Result<SharedCommerce.Application.Contracts.Appearance.MediaAssetReference>.Success(asset));
    }
}
