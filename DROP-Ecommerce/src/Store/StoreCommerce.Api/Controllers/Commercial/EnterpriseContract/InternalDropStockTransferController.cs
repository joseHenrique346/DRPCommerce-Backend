using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Api.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using StoreCommerce.Domain.Entity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/internal/drop-stock-transfers")]
[AllowAnonymous]
public sealed class InternalDropStockTransferController(IMediator mediator) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var result = await mediator.Send(new ListPendingDropStockTransfersQuery(), ct);

        return result.IsSuccess ? Ok(result) : this.ToIdentityActionResult(result);
    }

    [HttpPost("{operationId:guid}/ack")]
    public async Task<IActionResult> Acknowledge(Guid operationId, DropStockAcknowledgment request, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant)
            return NotFound();

        var result = await mediator.Send(new AcknowledgeDropStockTransferCommand(tenant, operationId, request.CatalogProductId), ct);

        return result.IsSuccess ? NoContent() : this.ToIdentityActionResult(result);
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return(DropStockReturn request, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant || tenant != request.EnterpriseId)
            return NotFound();

        var result = await mediator.Send(new ReturnDropStockCommand(request), ct);

        return result.IsSuccess ? NoContent() : this.ToIdentityActionResult(result);
    }
}
