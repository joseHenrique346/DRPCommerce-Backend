using SharedCommerce.Application.Security;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropOrderController(DropCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista os pedidos de drop do cliente.
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> Orders(CancellationToken ct)
    {
        return Ok(await db.DropOrders
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.DropEventId,
                x.TotalAmount,
                x.DropOrderStatusId,
                x.DropOrderPaymentStatusId
            })
            .ToListAsync(ct));
    }

    /// <summary>
    /// Consulta um pedido de drop do cliente.
    /// </summary>
    [HttpGet("orders/{id:long}")]
    public async Task<IActionResult> Order(long id, CancellationToken ct)
    {
        var value = await db.DropOrders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.TotalAmount,
                x.DropOrderStatusId,
                x.DropOrderPaymentStatusId,
                Items = x.ListDropOrderItem.Select(i => new
                {
                    i.ItemName,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice
                })
            })
            .SingleOrDefaultAsync(ct);

        return value is null ? NotFound() : Ok(value);
    }
}

