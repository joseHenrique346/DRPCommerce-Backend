using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Category;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/categories")]
public class CategoryController : BaseController<Category, CreateCategoryCommand, CreateListCategoryCommand, UpdateCategoryCommand, UpdateListCategoryCommand>
{
    public CategoryController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Consulta a árvore da loja, incluindo categorias inativas.
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTreeAsync(CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new GetCategoryTreeQuery(), ct));
    }

    /// <summary>
    /// Busca categorias por nome, pai e situação, com paginação.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> GetSearchAsync([FromQuery] SearchCategoriesQuery query, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(query, ct));
    }

    /// <summary>
    /// Altera a ativação da categoria. Filhos e produtos de uma ramificação inativa ficam ocultos na vitrine.
    /// </summary>
    [HttpPut("{id:long}/active")]
    public async Task<IActionResult> SetActiveAsync(long id, [FromBody] SetCategoryActiveRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new SetCategoryActiveCommand(id, request.IsActive), ct));
    }

    /// <summary>
    /// Atualiza a ordem de exibição de várias categorias em uma transação.
    /// </summary>
    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderAsync([FromBody] ReorderCategoriesCommand command, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(command, ct));
    }

    /// <summary>
    /// Exclui logicamente a categoria, com transferência opcional dos produtos para outra categoria ativa.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteCategoryAsync(long id, [FromQuery] long? moveProductsToCategoryId, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new DeleteCategoryCommand(id, moveProductsToCategoryId), ct));
    }

    /// <summary>
    /// Exclui categorias em lote, com transferência opcional dos produtos. Subcategorias precisam integrar o lote.
    /// </summary>
    [HttpDelete("delete-with-products")]
    public async Task<IActionResult> DeleteWithProductsAsync([FromBody] DeleteListCategoryCommand command, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(command, ct));
    }

    protected override CreateListCategoryCommand WrapCreateInRange(CreateCategoryCommand command)
    {
        return new CreateListCategoryCommand(new List<CreateCategoryCommand> { command });
    }

    protected override UpdateListCategoryCommand WrapUpdateInRange(UpdateCategoryCommand command)
    {
        return new UpdateListCategoryCommand(new List<UpdateCategoryCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListCategoryCommand(ids);
    }

    protected override IRequest<Result<List<Category>>> GetAllQuery()
    {
        return new GetAllCategoryQuery();
    }

    protected override IRequest<Result<Category>> GetByIdQuery(long id)
    {
        return new GetByIdCategoryQuery(id);
    }

    protected override IRequest<Result<List<Category>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdCategoryQuery(ids);
    }
}
