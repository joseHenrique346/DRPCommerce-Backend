using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using MediatR;

namespace StoreCommerce.Application.Features.Commands.Base.Handlers;

public abstract class BaseGetAllHandler<TRequest, TEntity>(IRepository<TEntity> repository)
    : IRequestHandler<TRequest, Result<List<TEntity>>>
    where TRequest : IRequest<Result<List<TEntity>>>
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;

    public virtual async Task<Result<List<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var entities = (await Repository.GetAllAsync(cancellationToken)).ToList();
        return Result<List<TEntity>>.Success(entities);
    }
}
