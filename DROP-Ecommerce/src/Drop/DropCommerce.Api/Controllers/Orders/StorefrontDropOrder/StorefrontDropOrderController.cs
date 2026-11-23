using MediatR;
using DropCommerce.Application.Features.Commands;
using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/{storeSlug}/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropOrderController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os pedidos de drop do cliente.
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> Orders(CancellationToken ct)
    {
        return Ok(await mediator.Send(new ListMyDropOrdersQuery(), ct));
    }

    /// <summary>
    /// Consulta um pedido de drop do cliente.
    /// </summary>
    [HttpGet("orders/{id:long}")]
    public async Task<IActionResult> Order(long id, CancellationToken ct)
    {
        var value = await mediator.Send(new GetMyDropOrderQuery(id), ct);

        return value is null ? NotFound() : Ok(value);
    }
}
