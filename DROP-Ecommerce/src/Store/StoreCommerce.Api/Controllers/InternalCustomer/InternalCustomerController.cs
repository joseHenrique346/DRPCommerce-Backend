using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Result;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalCustomerController(StoreCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Consulta um cliente para integração interna.
    /// </summary>
    [HttpGet("customers/get-by-id/{id:long}")]
    public async Task<IActionResult> Customer(long id, CancellationToken ct)
    {
        var value = await db.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                CustomerId = x.Id,
                x.EnterpriseId,
                x.FullName,
                x.IsActive
            })
            .SingleOrDefaultAsync(ct);

        return value is null ? NotFound() : Ok(Result<object>.Success(value));
    }

    /// <summary>
    /// Consulta clientes por identificadores para integração interna.
    /// </summary>
    [HttpPost("customers/get-list-by-list-id")]
    public async Task<IActionResult> Customers(List<long> ids, CancellationToken ct)
    {
        return Ok(Result<object>.Success(await db.Customers
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new
            {
                CustomerId = x.Id,
                x.EnterpriseId,
                x.FullName,
                x.IsActive
            })
            .ToListAsync(ct)));
    }
}

