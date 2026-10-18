using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers.Base;

public abstract class MappedCrudController<TEntity, TResponse, TCreateCommand, TCreateRangeCommand, TUpdateCommand, TUpdateRangeCommand>(IMediator mediator) : BaseController<TEntity, TCreateCommand, TCreateRangeCommand, TUpdateCommand, TUpdateRangeCommand>(mediator)
    where TEntity : BaseEntity
    where TCreateCommand : IRequest<Result<TEntity>>
    where TCreateRangeCommand : IRequest<Result<List<TEntity>>>
    where TUpdateCommand : IRequest<Result<TEntity>>
    where TUpdateRangeCommand : IRequest<Result<List<TEntity>>>
{
    protected abstract TResponse MapEntity(TEntity entity);

    protected override IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess && result.Content is TEntity entity)
            return this.ToActionResult(Result<TResponse>.Success(MapEntity(entity)));
        if (result.IsSuccess && result.Content is List<TEntity> entities)
            return this.ToActionResult(Result<List<TResponse>>.Success(entities.Select(MapEntity).ToList()));
        return base.HandleResult(result);
    }
}
