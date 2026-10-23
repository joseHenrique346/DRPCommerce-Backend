using SharedCommerce.Application.Security;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprise-domains")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class EnterpriseDomainController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os domínios da loja.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListDomainsQuery(), ct));
    }

    /// <summary>
    /// Cadastra um domínio e gera o desafio de verificação.
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> Add(DomainRegistrationRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new RegisterDomainCommand(request), ct));
    }

    /// <summary>
    /// Verifica a propriedade do domínio pelo DNS.
    /// </summary>
    [HttpPost("{id:long}/verify")]
    public async Task<IActionResult> Verify(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new VerifyDomainCommand(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }

    /// <summary>
    /// Define a ativação e o domínio principal da loja.
    /// </summary>
    [HttpPut("{id:long}/publication")]
    public async Task<IActionResult> Publish(long id, DomainActivationRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SetDomainActiveCommand(id, request), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }

    /// <summary>
    /// Exclui um domínio que não seja o principal.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteDomainCommand(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
