using DropCommerce.Infrastructure.Data.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
public sealed class StorefrontDropPurchaseController(DropCheckoutService checkout) : ControllerBase
{
    [HttpPost("reservations/{id:long}/checkout")]
    public async Task<IActionResult> Checkout(long id, CancellationToken ct)
    {
        return Ok(await checkout.CheckoutAsync(id, ct));
    }

    [HttpGet("purchases/{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        return Ok(await checkout.GetOrderAsync(id, ct));
    }
}

public sealed record AdvanceDropFulfillment(long StatusId);

[ApiController]
[Route("api/drop-purchases")]
[Authorize(Policy = SecurityNames.Backoffice)]
public sealed class DropPurchaseController(DropCheckoutService checkout) : ControllerBase
{
    [HttpPost("{id:long}/fulfillment")]
    public async Task<IActionResult> Fulfillment(long id, AdvanceDropFulfillment request, CancellationToken ct)
    {
        await checkout.AdvanceFulfillmentAsync(id, request.StatusId, ct);

        return NoContent();
    }
}
