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
[Route("api/identity/ownership-transfers")]
[Authorize(Policy = SecurityNames.GlobalUser)]
[EnableRateLimiting("auth")]
public sealed class OwnershipTransferAcceptanceController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Aceita a transferência e assume a propriedade da loja.
    /// </summary>
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(AcceptOwnershipTransferRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AcceptOwnershipTransferCommand(request), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}

