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
[Route("api/ownership-transfers")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class OwnershipTransferController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Solicita a transferência de propriedade da loja.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(OwnershipTransferRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new RequestOwnershipTransferCommand(request), ct));
    }

    /// <summary>
    /// Cancela uma transferência de propriedade pendente.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Revoke(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokeOwnershipTransferCommand(id), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}

