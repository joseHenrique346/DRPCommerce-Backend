using DropCommerce.Api.Extensions;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<TEntity, TCreateCommand, TCreateRangeCommand, TUpdateCommand, TUpdateRangeCommand> : ControllerBase
    where TEntity : BaseEntity
    where TCreateCommand : IRequest<Result<TEntity>>
    where TCreateRangeCommand : IRequest<Result<List<TEntity>>>
    where TUpdateCommand : IRequest<Result<TEntity>>
    where TUpdateRangeCommand : IRequest<Result<List<TEntity>>>
{
    protected readonly IMediator _mediator;

    protected BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddAsync([FromBody] TCreateCommand command)
    {
        var rangeCommand = WrapCreateInRange(command);
        return await AddRangeAsync(rangeCommand);
    }

    [HttpPost("add-range")]
    public async Task<IActionResult> AddRangeAsync([FromBody] TCreateRangeCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateAsync([FromBody] TUpdateCommand command)
    {
        var rangeCommand = WrapUpdateInRange(command);
        return await UpdateRangeAsync(rangeCommand);
    }

    [HttpPut("update-range")]
    public async Task<IActionResult> UpdateRangeAsync([FromBody] TUpdateRangeCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        return await DeleteRangeAsync(new List<long> { id });
    }

    [HttpDelete("delete-range")]
    public async Task<IActionResult> DeleteRangeAsync([FromBody] List<long> ids)
    {
        var command = DeleteRangeCommand(ids);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAsync()
    {
        var query = GetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<IActionResult> GetByIdAsync(long id)
    {
        var query = GetByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost("get-list-by-list-id")]
    public async Task<IActionResult> GetListByListIdAsync([FromBody] List<long> ids)
    {
        var query = GetListByListIdQuery(ids);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        return this.ToActionResult(result);
    }

    protected abstract TCreateRangeCommand WrapCreateInRange(TCreateCommand command);
    protected abstract TUpdateRangeCommand WrapUpdateInRange(TUpdateCommand command);
    protected abstract IRequest<Result<bool>> DeleteRangeCommand(List<long> ids);
    protected abstract IRequest<Result<List<TEntity>>> GetAllQuery();
    protected abstract IRequest<Result<TEntity>> GetByIdQuery(long id);
    protected abstract IRequest<Result<List<TEntity>>> GetListByListIdQuery(List<long> ids);
}
