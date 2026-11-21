using DropCommerce.Infrastructure.Data.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

public sealed record ReactivateDropCatalog(long ExpectedContractRevision);

[ApiController]
[Route("api/drop-catalog")]
[Authorize(Policy = SecurityNames.Backoffice)]
public sealed class DropCatalogController(DropCatalogService catalog, DropStockSagaService stock) : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(long? afterId, CancellationToken ct)
    {
        return Ok(await catalog.GetProductsAsync(afterId, ct));
    }

    [HttpGet("usage")]
    public async Task<IActionResult> GetUsage(CancellationToken ct)
    {
        return Ok(await catalog.GetUsageAsync(ct));
    }

    [HttpPost("products")]
    public async Task<IActionResult> Create(CreateDropCatalogProduct request, CancellationToken ct)
    {
        return Ok(await catalog.CreateAsync(request, ct));
    }

    [HttpPost("products/{id:long}/events")]
    public async Task<IActionResult> Associate(long id, AssociateDropCatalogProduct request, CancellationToken ct)
    {
        return Ok(await catalog.AssociateAsync(id, request, ct));
    }

    [HttpPost("reactivation/preview")]
    public async Task<IActionResult> Preview(ReactivateDropParticipations request, CancellationToken ct)
    {
        return Ok(await catalog.ReactivationAsync(request, false, ct));
    }

    [HttpPost("reactivation")]
    public async Task<IActionResult> Reactivate(ReactivateDropParticipations request, CancellationToken ct)
    {
        return Ok(await catalog.ReactivationAsync(request, true, ct));
    }

    [HttpDelete("products/{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await catalog.DeleteAsync(id, ct);

        return NoContent();
    }

    [HttpPost("products/{id:long}/reactivate")]
    public async Task<IActionResult> ReactivateProduct(long id, ReactivateDropCatalog request, CancellationToken ct)
    {
        await catalog.ReactivateCatalogAsync(id, request.ExpectedContractRevision, ct);

        return NoContent();
    }

    [HttpPost("products/{id:long}/return-to-store")]
    public async Task<IActionResult> Return(long id, CancellationToken ct)
    {
        return Accepted(await stock.ReturnAsync(id, ct));
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(long? beforeId, CancellationToken ct)
    {
        return Ok(await catalog.GetActivityAsync(beforeId, ct));
    }
}
