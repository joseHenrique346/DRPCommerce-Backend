using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/products/{productId:long}/variants")]
public sealed class ProductVariantController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(long productId, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetProductVariantsQuery(productId), ct));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(long productId, [FromBody] CreateVariantRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new CreateProductVariantCommand(productId, request), ct));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateAsync(long productId, long id, [FromBody] VariantDetailsRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new UpdateProductVariantCommand(productId, id, request), ct));
    }

    [HttpPut("{id:long}/pricing")]
    public async Task<IActionResult> UpdatePricingAsync(long productId, long id, [FromBody] VariantPricingRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new UpdateProductVariantPricingCommand(productId, id, request), ct));
    }

    [HttpPut("{id:long}/dimensions")]
    public async Task<IActionResult> UpdateDimensionsAsync(long productId, long id, [FromBody] VariantDimensionsRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new UpdateProductVariantDimensionsCommand(productId, id, request), ct));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync(long productId, long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new DeleteProductVariantCommand(productId, id), ct));
    }
}
