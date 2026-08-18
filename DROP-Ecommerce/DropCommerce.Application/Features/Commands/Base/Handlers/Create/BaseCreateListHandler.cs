using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using MediatR;

namespace DropCommerce.Application.Features.Commands.Base.Handlers;

public abstract class BaseCreateListHandler<TCommand, TRequest, TEntity>(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<TRequest, Result<List<TEntity>>>
    where TRequest : IRequest<Result<List<TEntity>>>
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract IReadOnlyCollection<TCommand> GetCommandList(TRequest request);
    protected abstract TEntity CreateEntity(TCommand command);

    protected virtual Task ValidateBusinessRulesAsync(TCommand command, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task BeforeCommitAsync(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task AfterCommitAsync(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task<Result<List<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var entities = new List<TEntity>();

        foreach (var command in GetCommandList(request))
        {
            await ValidateBusinessRulesAsync(command, cancellationToken);
            entities.Add(CreateEntity(command));
        }

        await Repository.AddRangeAsync(entities, cancellationToken);
        await BeforeCommitAsync(entities, cancellationToken);
        await UnitOfWork.CommitAsync(cancellationToken);
        await AfterCommitAsync(entities, cancellationToken);

        return Result<List<TEntity>>.Success(entities);
    }
}
