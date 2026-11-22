using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprise-plans")]
[AllowAnonymous]
public sealed class EnterprisePlanController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Catálogo público de planos, adicionais, preços mensais de referência e capacidades.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetEnterprisePlanCatalogQuery(), ct));
    }
}
