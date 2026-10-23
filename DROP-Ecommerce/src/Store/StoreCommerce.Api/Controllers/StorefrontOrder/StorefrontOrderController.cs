using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Security.Context;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront")]
[EnableRateLimiting("api")]
public sealed class StorefrontOrderController(StoreCommerceDbContext db, IdentityContext actor) : ControllerBase
{
    /// <summary>
    /// Lista os pedidos do cliente autenticado.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpGet("orders")]
    public async Task<IActionResult> Orders(CancellationToken ct)
    {
        var id = actor.CustomerId;

        return Ok(await db.Orders
            .AsNoTracking()
            .Where(x => x.CustomerId == id)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.OrderStatusId,
                x.OrderPaymentStatusId,
                x.TotalAmount,
                x.CreatedAt
            })
            .ToListAsync(ct));
    }

    /// <summary>
    /// Consulta um pedido do cliente autenticado.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpGet("orders/{id:long}")]
    public async Task<IActionResult> Order(long id, CancellationToken ct)
    {
        var customerId = actor.CustomerId;
        var value = await db.Orders
            .AsNoTracking()
            .Where(x => x.Id == id && x.CustomerId == customerId)
            .Select(x => new
            {
                x.Id,
                x.OrderStatusId,
                x.OrderPaymentStatusId,
                x.TotalAmount,
                x.CreatedAt,
                Items = x.ListOrderItem.Select(i => new
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

