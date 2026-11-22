using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedCommerce.Application.Security;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Security.Context;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontCouponController(IMediator mediator, IdentityContext actor) : ControllerBase
{
    /// <summary>
    /// Valida um código de cupom para o carrinho. Com cliente autenticado, também avalia clientes novos/recorrentes e uso único.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("coupons/validate")]
    public async Task<IActionResult> ValidateAsync([FromQuery] string code, [FromQuery] decimal subTotal, [FromQuery] int itemQuantity, CancellationToken ct)
    {
        // O cliente vem somente do token; a vitrine nunca aceita customerId informado pelo navegador.
        long? customerId = User.FindFirst(SecurityNames.Actor)?.Value == "customer" ? actor.CustomerId : null;

        return this.ToActionResult(await mediator.Send(new ValidateCouponQuery(code, subTotal, itemQuantity, customerId), ct));
    }
}
