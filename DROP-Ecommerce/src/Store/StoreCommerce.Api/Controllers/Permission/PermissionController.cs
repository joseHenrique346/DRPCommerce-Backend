using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionController(IMediator mediator) : ControllerBase
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionQuery(), cancellationToken);
        return result.IsSuccess
            ? this.ToActionResult(Result<List<PermissionResponse>>.Success(result.Content!.Select(MapEntity).ToList()))
            : this.ToActionResult(result);
    }

    [HttpGet("get-by-id/{id:long}")]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetByIdPermissionQuery(id), cancellationToken);
        return result.IsSuccess
            ? this.ToActionResult(Result<PermissionResponse>.Success(MapEntity(result.Content!)))
            : this.ToActionResult(result);
    }

    private static PermissionResponse MapEntity(Permission entity) =>
        new(entity.Id, entity.Code, entity.Name, entity.Description, entity.Module, entity.IsActive);
}
