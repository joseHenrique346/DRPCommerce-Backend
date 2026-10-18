using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontCustomerController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta o perfil do cliente autenticado nesta loja.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetCustomerQuery(), ct));
    }

    /// <summary>
    /// Atualiza o perfil do cliente autenticado nesta loja.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpPut("me")]
    public async Task<IActionResult> Update(CustomerProfileRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new UpdateCustomerProfileCommand(request), ct));
    }

    /// <summary>
    /// Desativa a conta do cliente nesta loja.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpDelete("me")]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateCustomerAccountCommand(), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}

