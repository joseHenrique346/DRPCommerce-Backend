using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/store-activity")]
[Authorize(Policy = SecurityNames.GlobalUser)]
public sealed class StoreActivityController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Histórico somente leitura da loja atual, com permissão própria e filtros paginados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] StoreActivityQueryRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetStoreActivityQuery(request), ct));
    }
}
