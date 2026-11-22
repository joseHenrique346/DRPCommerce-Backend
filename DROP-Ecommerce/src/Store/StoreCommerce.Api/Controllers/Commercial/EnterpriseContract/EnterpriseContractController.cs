using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprises/current/contract")]
[Authorize(Policy = SecurityNames.GlobalUser)]
public sealed class EnterpriseContractController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta o contrato efetivo e os direitos da loja selecionada.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetCurrentEnterpriseContractQuery(), ct));
    }

    /// <summary>
    /// Consulta consumo local e capacidade informativa; não reserva vagas nem aplica limites.
    /// </summary>
    [HttpGet("usage")]
    public async Task<IActionResult> GetUsage(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetEnterpriseContractUsageQuery(), ct));
    }

    /// <summary>
    /// Registra a escolha do dono, sem conceder plano ou iniciar cobrança.
    /// </summary>
    [HttpPost("requests")]
    [Authorize(Policy = SecurityNames.PrimaryOwner)]
    public async Task<IActionResult> RequestChange(RequestEnterpriseContractChangeRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RequestEnterpriseContractChangeCommand(request), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return AcceptedAtAction(nameof(GetRequest), new { id = result.Content!.Id }, result.Content);
    }

    /// <summary>
    /// Consulta uma solicitação da própria loja; a escolha nunca é prova de pagamento.
    /// </summary>
    [HttpGet("requests/{id:long}")]
    [Authorize(Policy = SecurityNames.PrimaryOwner)]
    public async Task<IActionResult> GetRequest(long id, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetEnterpriseContractChangeQuery(id), ct));
    }

    /// <summary>
    /// Calcula o impacto de uma oferta sobre os produtos, sem conceder o plano.
    /// </summary>
    [HttpPost("product-impact")]
    [Authorize(Policy = SecurityNames.PrimaryOwner)]
    public async Task<IActionResult> PreviewProductImpact(ProductContractImpactRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new PreviewProductContractImpactQuery(request), ct));
    }

    /// <summary>
    /// Exibe os acessos a escolher e os domínios afetados pela oferta, sem alterar o contrato.
    /// </summary>
    [HttpPost("access-impact")]
    [Authorize(Policy = SecurityNames.PrimaryOwner)]
    public async Task<IActionResult> PreviewAccessImpact(AccessContractImpactRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new PreviewAccessContractImpactQuery(request), ct));
    }
}
