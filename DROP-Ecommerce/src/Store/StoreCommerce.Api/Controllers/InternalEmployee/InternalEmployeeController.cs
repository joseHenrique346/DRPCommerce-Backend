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
public sealed class InternalEmployeeController(StoreCommerceDbContext db) : ControllerBase
{
    /// <summary>
    /// Consulta um funcionário para integração interna.
    /// </summary>
    [HttpGet("employees/get-by-id/{id:long}")]
    public async Task<IActionResult> Employee(long id, CancellationToken ct)
    {
        var value = await db.Employees
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                EmployeeId = x.Id,
                x.EnterpriseId,
                x.FullName,
                x.IsActive
            })
            .SingleOrDefaultAsync(ct);

        return value is null ? NotFound() : Ok(Result<object>.Success(value));
    }

    /// <summary>
    /// Consulta funcionários por identificadores para integração interna.
    /// </summary>
    [HttpPost("employees/get-list-by-list-id")]
    public async Task<IActionResult> Employees(List<long> ids, CancellationToken ct)
    {
        return Ok(Result<object>.Success(await db.Employees
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new
            {
                EmployeeId = x.Id,
                x.EnterpriseId,
                x.FullName,
                x.IsActive
            })
            .ToListAsync(ct)));
    }
}

