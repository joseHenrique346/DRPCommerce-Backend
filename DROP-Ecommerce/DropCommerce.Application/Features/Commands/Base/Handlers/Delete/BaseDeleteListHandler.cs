using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using MediatR;

namespace DropCommerce.Application.Features.Commands.Base.Handlers;

public abstract class BaseDeleteListHandler<TRequest, TEntity>(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<TRequest, Result<bool>>
    where TRequest : IRequest<Result<bool>>
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract IReadOnlyCollection<long> GetIdList(TRequest request);

    protected virtual Task ValidateBusinessRulesAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task BeforeCommitAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task AfterCommitAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task<Result<bool>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var listId = GetIdList(request);

        await ValidateBusinessRulesAsync(listId, cancellationToken);
        await Repository.DeleteRangeAsync(listId, cancellationToken);
        await BeforeCommitAsync(listId, cancellationToken);
        await UnitOfWork.CommitAsync(cancellationToken);
        await AfterCommitAsync(listId, cancellationToken);

        return Result<bool>.Success(true);
    }
}
