using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontEnterpriseController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta os dados públicos e as regras de venda da loja acessada.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("store")]
    public async Task<IActionResult> Store(CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetPublicStoreInfoQuery(), ct));
    }
}
