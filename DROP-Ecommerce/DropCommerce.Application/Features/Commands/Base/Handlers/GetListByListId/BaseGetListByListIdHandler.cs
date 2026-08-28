using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using MediatR;

namespace DropCommerce.Application.Features.Commands.Base.Handlers;

public abstract class BaseGetListByListIdHandler<TRequest, TEntity>(IRepository<TEntity> repository)
    : IRequestHandler<TRequest, Result<List<TEntity>>>
    where TRequest : IRequest<Result<List<TEntity>>>
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;

    protected abstract IReadOnlyCollection<long> GetListByListId(TRequest request);

    public virtual async Task<Result<List<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var entities = await Repository.GetListByListIdAsync(GetListByListId(request).ToList(), cancellationToken);

        return Result<List<TEntity>>.Success(entities.ToList());
    }
}
