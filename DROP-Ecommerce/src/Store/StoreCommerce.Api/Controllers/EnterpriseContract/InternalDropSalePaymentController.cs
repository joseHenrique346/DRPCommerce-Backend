using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Payments;

namespace StoreCommerce.Api.Controllers;

public sealed record DropSaleAcknowledgment(long Revision, bool FulfillmentClosed, bool RequiresReview = false);

[ApiController]
[AllowAnonymous]
[Route("api/internal/drop-sale-payments")]
public sealed class InternalDropSalePaymentController(DropSalePaymentService payments, StoreCommerceDbContext db) : ControllerBase
{
    private long Tenant => HttpContext.Items[SecurityNames.TrustedServiceTenant] is long tenant ? tenant : 0;

    [HttpGet("customers/{id:long}/profile")]
    public async Task<IActionResult> Profile(long id, CancellationToken ct)
    {
        return Tenant == 0 ? NotFound() : Ok(Result<DropBuyerProfile>.Success(await payments.ProfileAsync(Tenant, id, ct)));
    }

    [HttpPost]
    public async Task<IActionResult> Start(StartDropSalePayment request, CancellationToken ct)
    {
        return Tenant == 0 ? NotFound() : Ok(Result<DropSalePaymentReceipt>.Success(await payments.StartAsync(Tenant, request, ct)));
    }

    [HttpGet("{operationId:guid}")]
    public async Task<IActionResult> Get(Guid operationId, CancellationToken ct)
    {
        return Tenant == 0 ? NotFound() : Ok(Result<DropSalePaymentReceipt>.Success(await payments.ReconcileAsync(Tenant, operationId, ct)));
    }

    [HttpGet("pending-receipts")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var rows = await db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
            .Where(Intent => Intent.AcknowledgedRevision < Intent.Revision)
            .OrderBy(Intent => Intent.Id).Take(50).ToListAsync(ct);

        return Ok(Result<IReadOnlyList<DropSalePaymentReceipt>>.Success(rows.Select(DropSalePaymentService.Receipt).ToList()));
    }

    [HttpPost("{operationId:guid}/ack")]
    public async Task<IActionResult> Acknowledge(Guid operationId, DropSaleAcknowledgment request, CancellationToken ct)
    {
        if (Tenant == 0)
            return NotFound();

        await payments.AcknowledgeAsync(Tenant, operationId, request.Revision, request.FulfillmentClosed, request.RequiresReview, ct);

        return NoContent();
    }
}
