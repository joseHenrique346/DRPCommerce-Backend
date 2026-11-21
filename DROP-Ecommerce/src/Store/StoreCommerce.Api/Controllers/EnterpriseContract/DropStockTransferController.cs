using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Contracts;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/products/{id:long}/drop-stock-transfer")]
[Authorize(Policy = SecurityNames.Backoffice)]
public sealed class DropStockTransferController(DropStockTransferService transfers) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Transfer(long id, StartDropStockTransfer request, CancellationToken ct)
    {
        return Accepted(await transfers.StartAsync(id, request, ct));
    }
}

public sealed record DropStockAcknowledgment(long CatalogProductId);

[ApiController]
[Route("api/internal/drop-stock-transfers")]
[AllowAnonymous]
public sealed class InternalDropStockTransferController(StoreCommerceDbContext db, DropStockTransferService transfers) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var rows = await db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking()
            .Where(Transfer => Transfer.ImportedAt == null && Transfer.ReturnedAt == null)
            .OrderBy(Transfer => Transfer.Id).Take(50).Select(Transfer => Transfer.SnapshotJson).ToListAsync(ct);

        return Ok(Result<IReadOnlyList<DropStockExport>>.Success(rows.Select(Row => JsonSerializer.Deserialize<DropStockExport>(Row)!).ToList()));
    }

    [HttpPost("{operationId:guid}/ack")]
    public async Task<IActionResult> Acknowledge(Guid operationId, DropStockAcknowledgment request, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant)
            return NotFound();

        await transfers.AcknowledgeAsync(tenant, operationId, request.CatalogProductId, ct);

        return NoContent();
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return(DropStockReturn request, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant || tenant != request.EnterpriseId)
            return NotFound();

        await transfers.ReturnAsync(request, ct);

        return NoContent();
    }
}
