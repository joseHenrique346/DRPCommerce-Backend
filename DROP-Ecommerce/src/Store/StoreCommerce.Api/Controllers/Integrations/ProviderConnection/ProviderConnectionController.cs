using StoreCommerce.Domain.Enums.Integrations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/provider-connections")]
public sealed class ProviderConnectionController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista metadados das conexões da loja autenticada. Não retorna tokens ou credenciais criptografadas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetProviderConnectionsQuery(), ct));
    }

    /// <summary>
    /// Desconecta localmente e apaga os tokens. Preserva referências históricas e impede a renovação em andamento.
    /// A revogação remota será responsabilidade do adapter de autorização do provedor.
    /// </summary>
    [HttpDelete("{provider}/{environment}")]
    public async Task<IActionResult> DisconnectAsync(string provider, IntegrationEnvironment environment, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new DisconnectProviderConnectionCommand(provider, environment), ct));
    }
}
