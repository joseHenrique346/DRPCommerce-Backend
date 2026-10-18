using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprises")]
[Authorize(Policy = SecurityNames.GlobalUser)]
public sealed class EnterpriseController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista as lojas vinculadas ao usuário.
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new ListEnterprisesQuery(), ct));
    }

    /// <summary>
    /// Cadastra uma loja com seu dono principal e subdomínio.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(EnterpriseRegistrationRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new RegisterEnterpriseCommand(request), ct));
    }

    /// <summary>
    /// Atualiza os dados da loja selecionada.
    /// </summary>
    [HttpPut("current")]

    [Authorize(Policy = SecurityNames.PrimaryOwner)]
    public async Task<IActionResult> Update(EnterpriseUpdateRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new UpdateCurrentEnterpriseCommand(request), ct));
    }

    /// <summary>
    /// Ativa ou desativa uma loja do dono principal.
    /// </summary>
    [HttpPut("{id:long}/activation")]
    public async Task<IActionResult> Activation(long id, [FromBody] bool active, CancellationToken ct)
    {
        var result = await mediator.Send(new SetEnterpriseActiveCommand(id, active), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}
