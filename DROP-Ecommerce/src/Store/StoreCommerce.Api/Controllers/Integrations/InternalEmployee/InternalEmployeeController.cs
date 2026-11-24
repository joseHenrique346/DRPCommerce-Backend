using MediatR;
using StoreCommerce.Application.Features.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Api.Controllers;

// Service authentication is performed by StoreTenantResolver BEFORE authentication/authorization.
// No public endpoint accepts X-Service-Key or X-Enterprise-Id as authority.
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalEmployeeController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta um funcionário para integração interna.
    /// </summary>
    [HttpGet("employees/get-by-id/{id:long}")]
    public async Task<IActionResult> Employee(long id, CancellationToken ct)
    {
        var value = await mediator.Send(new GetInternalEmployeeQuery(id), ct);

        return value is null ? NotFound() : Ok(Result<object>.Success(value));
    }

    /// <summary>
    /// Consulta funcionários por identificadores para integração interna.
    /// </summary>
    [HttpPost("employees/get-list-by-list-id")]
    public async Task<IActionResult> Employees(List<long> ids, CancellationToken ct)
    {
        return Ok(Result<object>.Success(await mediator.Send(new ListInternalEmployeesQuery(ids), ct)));
    }
}
