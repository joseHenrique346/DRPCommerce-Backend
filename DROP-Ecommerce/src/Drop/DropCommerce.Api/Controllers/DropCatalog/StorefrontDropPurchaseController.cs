using MediatR;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Contracts;
using DropCommerce.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/{storeSlug}/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
public sealed class StorefrontDropPurchaseController(IMediator mediator) : ControllerBase
{
    [HttpPost("reservations/{id:long}/checkout")]
    public async Task<IActionResult> Checkout(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new CheckoutDropPurchaseCommand(id), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpGet("purchases/{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDropPurchaseQuery(id), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }
}
