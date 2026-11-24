using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Result;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalProductController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta os SKUs e opções do produto para integrações autenticadas da loja.
    /// </summary>
    [HttpGet("products/{id:long}/variants")]
    public async Task<IActionResult> GetVariantsAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetProductVariantsQuery(id), ct));
    }

    /// <summary>
    /// Consulta um produto para integração interna.
    /// </summary>
    [HttpGet("products/get-by-id/{id:long}")]
    public async Task<IActionResult> Product(long id, CancellationToken ct)
    {
        var value = await mediator.Send(new GetInternalProductQuery(id), ct);

        return value is null ? NotFound() : Ok(Result<object>.Success(value));
    }

    /// <summary>
    /// Consulta produtos por identificadores para integração interna.
    /// </summary>
    [HttpPost("products/get-list-by-list-id")]
    public async Task<IActionResult> Products(List<long> ids, CancellationToken ct)
    {
        return Ok(Result<object>.Success(await mediator.Send(new ListInternalProductsQuery(ids), ct)));
    }
}
