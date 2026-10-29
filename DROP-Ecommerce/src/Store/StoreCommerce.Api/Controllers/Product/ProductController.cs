using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/products")]
public class ProductController : MappedCrudController<Product, ProductResponse, CreateProductCommand, CreateListProductCommand, UpdateProductCommand, UpdateListProductCommand>
{
    public ProductController(IMediator mediator) : base(mediator)
    {
    }

    protected override ProductResponse MapEntity(Product entity)
    {
        return ProductResponse.FromEntity(entity);
    }

    /// <summary>
    /// Pesquisa produtos e SKUs por categoria, marca, preço, disponibilidade e situação.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> GetSearchAsync([FromQuery] ProductSearchRequest query, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new SearchProductsQuery(query), ct));
    }

    /// <summary>
    /// Atualiza os dados descritivos do produto e do SKU padrão.
    /// </summary>
    [HttpPut("{id:long}/details")]
    public async Task<IActionResult> UpdateDetailsAsync(long id, [FromBody] ProductDetailsRequest request, CancellationToken ct)
    {
        return HandleResult(await _mediator.Send(new UpdateProductDetailsCommand(id, request.Name, request.Slug,
            request.Description, request.SKU, request.BarCode, request.Brand, request.ImageUrls, request.IsDigital), ct));
    }

    /// <summary>
    /// Altera preço e custo; exige products.change_price e respeita os valores herdados pelos SKUs.
    /// </summary>
    [HttpPut("{id:long}/pricing")]
    public async Task<IActionResult> UpdatePricingAsync(long id, [FromBody] ProductPricingRequest request, CancellationToken ct)
    {
        return HandleResult(await _mediator.Send(new UpdateProductPricingCommand(id, request.Price, request.CostPrice), ct));
    }

    /// <summary>
    /// Altera peso e dimensões padrão do produto.
    /// </summary>
    [HttpPut("{id:long}/dimensions")]
    public async Task<IActionResult> UpdateDimensionsAsync(long id, [FromBody] ProductDimensionsRequest request, CancellationToken ct)
    {
        return HandleResult(await _mediator.Send(new UpdateProductDimensionsCommand(id, request.Weight, request.Height, request.Width, request.Length), ct));
    }

    /// <summary>
    /// Move o produto para uma categoria ativa da loja.
    /// </summary>
    [HttpPut("{id:long}/category")]
    public async Task<IActionResult> ChangeCategoryAsync(long id, [FromBody] ProductCategoryRequest request, CancellationToken ct)
    {
        return HandleResult(await _mediator.Send(new ChangeProductCategoryCommand(id, request.CategoryId, request.SupplierId), ct));
    }

    /// <summary>
    /// Ativa ou desativa o produto sem alterar seus saldos.
    /// </summary>
    [HttpPut("{id:long}/active")]
    public async Task<IActionResult> SetActiveAsync(long id, [FromBody] ProductActiveRequest request, CancellationToken ct)
    {
        return HandleResult(await _mediator.Send(new SetProductActiveCommand(id, request.IsActive), ct));
    }

    /// <summary>
    /// Converte um produto simples em produto com variantes. O SKU padrão deve estar sem saldo ou reservas.
    /// </summary>
    [HttpPut("{id:long}/variants/enable")]
    public async Task<IActionResult> EnableVariantsAsync(long id, CancellationToken ct)
    {
        return HandleResult(await _mediator.Send(new EnableProductVariantsCommand(id), ct));
    }

    protected override CreateListProductCommand WrapCreateInRange(CreateProductCommand command)
    {
        return new CreateListProductCommand(new List<CreateProductCommand> { command });
    }

    protected override UpdateListProductCommand WrapUpdateInRange(UpdateProductCommand command)
    {
        return new UpdateListProductCommand(new List<UpdateProductCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListProductCommand(ids);
    }

    protected override IRequest<Result<List<Product>>> GetAllQuery()
    {
        return new GetAllProductQuery();
    }

    protected override IRequest<Result<Product>> GetByIdQuery(long id)
    {
        return new GetByIdProductQuery(id);
    }

    protected override IRequest<Result<List<Product>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdProductQuery(ids);
    }
}
