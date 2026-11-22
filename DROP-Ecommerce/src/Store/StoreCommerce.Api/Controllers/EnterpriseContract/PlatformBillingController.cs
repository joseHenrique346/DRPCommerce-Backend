using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using System.Text.Json;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprises/current/billing")]
[Authorize(Policy = SecurityNames.GlobalUser)]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class PlatformBillingController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlatformBillingQuery(), ct);

        return this.ToIdentityActionResult(result);
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> Invoices([FromQuery] long? beforeId, CancellationToken ct)
    {
        var result = await mediator.Send(new ListPlatformBillingInvoicesQuery(beforeId), ct);

        return this.ToIdentityActionResult(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(StartPlatformBillingRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new StartPlatformBillingCommand(request), ct);

        return result.IsSuccess ? Accepted(result.Content) : this.ToIdentityActionResult(result);
    }

    [HttpPost("reconcile")]
    public async Task<IActionResult> Reconcile(CancellationToken ct)
    {
        var result = await mediator.Send(new ReconcilePlatformBillingCommand(), ct);

        return this.ToIdentityActionResult(result);
    }

    [HttpPut("pending-team")]
    public async Task<IActionResult> SelectTeam(PlatformBillingTeamSelection request, CancellationToken ct)
    {
        var result = await mediator.Send(new SelectPlatformBillingTeamCommand(request), ct);

        return result.IsSuccess ? NoContent() : this.ToIdentityActionResult(result);
    }
}
