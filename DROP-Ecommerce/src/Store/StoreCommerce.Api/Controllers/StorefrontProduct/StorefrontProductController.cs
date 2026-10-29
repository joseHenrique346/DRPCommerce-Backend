using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Contracts.Product;

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

    /// <summary>
    /// Pesquisa o catálogo público com filtros e resposta paginada.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("products/search")]
    public async Task<IActionResult> SearchAsync([FromQuery] ProductSearchRequest query, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new SearchStorefrontProductsQuery(query), ct));
    }

    /// <summary>
    /// Consulta um produto e seus SKUs disponíveis pelo slug, incluindo opções e preços efetivos.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("products/by-slug/{slug}")]
    public async Task<IActionResult> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetProductBySlugQuery(slug), ct));
    }
}

