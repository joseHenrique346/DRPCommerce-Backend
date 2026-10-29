using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("variants/{id:long}")]
    public async Task<IActionResult> GetStockAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetVariantStockQuery(id), ct));
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovementsAsync([FromQuery] GetStockMovementsQuery query, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(query, ct));
    }

    [HttpPut("variants/{id:long}/adjust")]
    public async Task<IActionResult> AdjustAsync(long id, [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new AdjustStockCommand(id, request), ct));
    }

    [HttpPost("reservations")]
    public async Task<IActionResult> ReserveAsync([FromBody] ReserveStockRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new ReserveStockCommand(request), ct));
    }

    [HttpGet("reservations/{id:long}")]
    public async Task<IActionResult> GetReservationAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetStockReservationQuery(id), ct));
    }

    [HttpPut("reservations/{id:long}/release")]
    public async Task<IActionResult> ReleaseAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new ReleaseStockCommand(id), ct));
    }

    [HttpPut("reservations/{id:long}/commit")]
    public async Task<IActionResult> CommitAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new CommitStockCommand(id), ct));
    }
}
