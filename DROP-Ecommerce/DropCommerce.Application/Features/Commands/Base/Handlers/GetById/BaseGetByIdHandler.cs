using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using MediatR;

namespace DropCommerce.Application.Features.Commands.Base.Handlers;

public abstract class BaseGetByIdHandler<TRequest, TEntity>(IRepository<TEntity> repository)
    : IRequestHandler<TRequest, Result<TEntity>>
    where TRequest : IRequest<Result<TEntity>>
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;

    protected abstract long GetById(TRequest request);

    protected virtual string NotFoundMessage(long id) =>
        $"{typeof(TEntity).Name} com id {id} não encontrado.";

    public virtual async Task<Result<TEntity>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var id = GetById(request);
        var entity = await Repository.GetByIdAsync(id, cancellationToken);

        return entity is null
            ? Result<TEntity>.Failure(NotFoundMessage(id))
            : Result<TEntity>.Success(entity);
    }
}
