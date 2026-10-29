using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedCommerce.Application.Security;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

/// <summary>
/// Carrinho do cliente autenticado. Todas as respostas trazem o carrinho atualizado, com estoque e totais dos itens selecionados.
/// </summary>
[ApiController]
[Route("api/storefront/cart")]
[EnableRateLimiting("api")]
[Authorize(Policy = SecurityNames.Storefront)]
public sealed class StorefrontCartController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta o carrinho. Itens sem estoque ou indisponíveis vêm com requiresDecision = true.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string? couponCode, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetCartQuery(couponCode), ct));
    }

    /// <summary>
    /// Adiciona um produto (ou soma a quantidade do SKU que já está no carrinho).
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItemAsync([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new AddCartItemCommand(request.ProductId, request.ProductVariantId, request.Quantity), ct));
    }

    /// <summary>
    /// Altera a quantidade de um item.
    /// </summary>
    [HttpPut("items/{id:long}")]
    public async Task<IActionResult> UpdateItemAsync(long id, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new UpdateCartItemQuantityCommand(id, request.Quantity), ct));
    }

    /// <summary>
    /// Remove um item do carrinho.
    /// </summary>
    [HttpDelete("items/{id:long}")]
    public async Task<IActionResult> RemoveItemAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new RemoveCartItemsCommand([id]), ct));
    }

    /// <summary>
    /// Remove vários itens do carrinho.
    /// </summary>
    [HttpDelete("items")]
    public async Task<IActionResult> RemoveItemsAsync([FromBody] List<long> ids, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new RemoveCartItemsCommand(ids), ct));
    }

    /// <summary>
    /// Esvazia o carrinho.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearAsync(CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new ClearCartCommand(), ct));
    }

    /// <summary>
    /// Marca ou desmarca itens para a compra. Sem itemIds, aplica a todos.
    /// </summary>
    [HttpPut("selection")]
    public async Task<IActionResult> SetSelectionAsync([FromBody] CartSelectionRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new SetCartSelectionCommand(request.ItemIds, request.Selected), ct));
    }

    /// <summary>
    /// Mantém um item sem estoque no carrinho, fora da compra, aguardando reposição.
    /// </summary>
    [HttpPut("items/{id:long}/await-stock")]
    public async Task<IActionResult> AwaitStockAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new AwaitCartItemStockCommand(id), ct));
    }
}
