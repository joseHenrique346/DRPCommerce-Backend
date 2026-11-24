using MediatR;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Contracts;
using DropCommerce.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/drop-catalog")]
[Authorize(Policy = SecurityNames.Backoffice)]
public sealed class DropCatalogController(IMediator mediator) : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(long? afterId, CancellationToken ct)
    {
        var result = await mediator.Send(new ListDropCatalogProductsQuery(afterId), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpGet("usage")]
    public async Task<IActionResult> GetUsage(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDropCatalogUsageQuery(), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpPost("products")]
    public async Task<IActionResult> Create(CreateDropCatalogProduct request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateDropCatalogProductCommand(request), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpPost("products/{id:long}/events")]
    public async Task<IActionResult> Associate(long id, AssociateDropCatalogProduct request, CancellationToken ct)
    {
        var result = await mediator.Send(new AssociateDropCatalogProductCommand(id, request), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpPost("reactivation/preview")]
    public async Task<IActionResult> Preview(ReactivateDropParticipations request, CancellationToken ct)
    {
        var result = await mediator.Send(new PreviewDropReactivationQuery(request), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpPost("reactivation")]
    public async Task<IActionResult> Reactivate(ReactivateDropParticipations request, CancellationToken ct)
    {
        var result = await mediator.Send(new ReactivateDropParticipationsCommand(request), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }

    [HttpDelete("products/{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteDropCatalogProductCommand(id), ct);

        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    [HttpPost("products/{id:long}/reactivate")]
    public async Task<IActionResult> ReactivateProduct(long id, ReactivateDropCatalog request, CancellationToken ct)
    {
        var result = await mediator.Send(new ReactivateDropCatalogProductCommand(id, request.ExpectedContractRevision), ct);

        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    [HttpPost("products/{id:long}/return-to-store")]
    public async Task<IActionResult> Return(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new ReturnDropCatalogStockCommand(id), ct);

        return result.IsSuccess ? Accepted(result.Content) : this.ToActionResult(result);
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(long? beforeId, CancellationToken ct)
    {
        var result = await mediator.Send(new ListDropCommercialActivityQuery(beforeId), ct);

        return result.IsSuccess ? Ok(result.Content) : this.ToActionResult(result);
    }
}
