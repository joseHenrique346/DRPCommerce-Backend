using MediatR;
using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
[Authorize(Policy = SecurityNames.Storefront)]
public sealed class StorefrontOrderController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Finaliza a compra dos itens selecionados do carrinho. Repetir a mesma idempotencyKey devolve o mesmo pedido.
    /// A resposta traz a cobrança (link, código Pix ou instruções) e o prazo para pagamento.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckoutAsync([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new PlaceOrderCommand(request), ct));
    }

    /// <summary>
    /// Lista os pedidos do cliente autenticado.
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> OrdersAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        return this.ToActionResult(await mediator.Send(new GetMyOrdersQuery(page, pageSize), ct));
    }

    /// <summary>
    /// Consulta um pedido do cliente autenticado, com itens, cobrança e histórico.
    /// </summary>
    [HttpGet("orders/{id:long}")]
    public async Task<IActionResult> OrderAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetMyOrderQuery(id), ct));
    }

    /// <summary>
    /// Cancela um pedido que ainda aguarda pagamento. O estoque reservado é liberado.
    /// </summary>
    [HttpPost("orders/{id:long}/cancel")]
    public async Task<IActionResult> CancelAsync(long id, [FromBody] CancelOrderRequest? request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new CancelMyOrderCommand(id, request?.Reason), ct));
    }
}
