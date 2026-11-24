using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

// Autorização pelo StoreOperationFilter: exige settings.manage ou o vínculo de dono principal.
[ApiController]
[Route("api/store-settings")]
public sealed class StoreSettingsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta as configurações da loja atual (retorna o padrão quando ainda não configurada).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetStoreSettingsQuery(), ct));
    }

    /// <summary>
    /// Atualiza as configurações da loja atual.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateStoreSettingsCommand command, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(command, ct));
    }
}
