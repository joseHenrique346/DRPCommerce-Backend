using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/storefront/categories")]
[EnableRateLimiting("api")]
public sealed class StorefrontCategoryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta a árvore visível da loja, ordenada por ordem de exibição.
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTreeAsync(CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetCategoryTreeQuery(true), ct));
    }

    /// <summary>
    /// Consulta uma categoria pelo slug quando ela e todos os seus ancestrais estão ativos.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetCategoryBySlugQuery(slug), ct));
    }
}
