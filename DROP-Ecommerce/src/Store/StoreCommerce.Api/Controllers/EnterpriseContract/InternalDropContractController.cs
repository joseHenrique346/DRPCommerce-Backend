using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalDropContractController(IMediator mediator) : ControllerBase
{
    [HttpGet("enterprises/{id:long}/drop-contract")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant || tenant != id)
            return NotFound();

        var result = await mediator.Send(new GetDropCommercialRightsQuery(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return result.Content is null ? NotFound() : Ok(result);
    }

    [HttpGet("drop-contracts/pending")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var result = await mediator.Send(new ListPendingDropContractsQuery(), ct);

        return result.IsSuccess ? Ok(result) : this.ToIdentityActionResult(result);
    }

    [HttpPost("enterprises/{id:long}/drop-contract/ack")]
    public async Task<IActionResult> Acknowledge(long id, AcknowledgeDropContractRequest request, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant || tenant != id)
            return NotFound();

        var result = await mediator.Send(new AcknowledgeDropContractCommand(id, request.Revision), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return result.Content ? NoContent() : Conflict();
    }
}
