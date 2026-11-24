using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/internal/drop-sale-payments")]
public sealed class InternalDropSalePaymentController(IMediator mediator) : ControllerBase
{
    private long Tenant => HttpContext.Items[SecurityNames.TrustedServiceTenant] is long tenant ? tenant : 0;

    [HttpGet("customers/{id:long}/profile")]
    public async Task<IActionResult> Profile(long id, CancellationToken ct)
    {
        if (Tenant == 0)
            return NotFound();

        var result = await mediator.Send(new GetDropBuyerProfileQuery(Tenant, id), ct);

        return result.IsSuccess ? Ok(result) : this.ToIdentityActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Start(StartDropSalePayment request, CancellationToken ct)
    {
        if (Tenant == 0)
            return NotFound();

        var result = await mediator.Send(new StartDropSalePaymentCommand(Tenant, request), ct);

        return result.IsSuccess ? Ok(result) : this.ToIdentityActionResult(result);
    }

    [HttpGet("{operationId:guid}")]
    public async Task<IActionResult> Get(Guid operationId, CancellationToken ct)
    {
        if (Tenant == 0)
            return NotFound();

        var result = await mediator.Send(new ReconcileDropSalePaymentCommand(Tenant, operationId), ct);

        return result.IsSuccess ? Ok(result) : this.ToIdentityActionResult(result);
    }

    [HttpGet("pending-receipts")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var result = await mediator.Send(new ListPendingDropPaymentReceiptsQuery(), ct);

        return result.IsSuccess ? Ok(result) : this.ToIdentityActionResult(result);
    }

    [HttpPost("{operationId:guid}/ack")]
    public async Task<IActionResult> Acknowledge(Guid operationId, DropSaleAcknowledgment request, CancellationToken ct)
    {
        if (Tenant == 0)
            return NotFound();

        var result = await mediator.Send(new AcknowledgeDropPaymentCommand(Tenant, operationId, request.Revision, request.FulfillmentClosed, request.RequiresReview), ct);

        return result.IsSuccess ? NoContent() : this.ToIdentityActionResult(result);
    }
}
