using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontProductController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os produtos disponíveis na loja.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetStorefrontProductsQuery(page, pageSize), ct));
    }
}

