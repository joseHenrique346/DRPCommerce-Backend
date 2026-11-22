using MediatR;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Contracts;
using DropCommerce.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/drop-purchases")]
[Authorize(Policy = SecurityNames.Backoffice)]
public sealed class DropPurchaseController(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:long}/fulfillment")]
    public async Task<IActionResult> Fulfillment(long id, AdvanceDropFulfillment request, CancellationToken ct)
    {
        var result = await mediator.Send(new AdvanceDropFulfillmentCommand(id, request.StatusId), ct);

        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }
}
