using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using MediatR;

namespace DropCommerce.Application.Features.Commands.Base.Handlers;

public abstract class BaseUpdateListHandler<TCommand, TRequest, TEntity>(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<TRequest, Result<List<TEntity>>>
    where TRequest : IRequest<Result<List<TEntity>>>
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract IReadOnlyCollection<TCommand> GetCommandList(TRequest request);
    protected abstract long GetById(TCommand command);
    protected abstract void ApplyChanges(TEntity entity, TCommand command);

    protected virtual string NotFoundMessage(long id) =>
        $"{typeof(TEntity).Name} com id {id} não encontrado.";

    protected virtual Task ValidateBusinessRulesAsync(TEntity entity, TCommand command, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task BeforeCommitAsync(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) => Task.CompletedTask;

    protected virtual Task AfterCommitAsync(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task<Result<List<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var commands = GetCommandList(request);
        var listId = commands.Select(GetById).ToList();

        if (listId.Count != listId.Distinct().Count())
            return Result<List<TEntity>>.Failure("A lista contém ids duplicados.");

        var storedEntities = (await Repository.GetListByListIdAsync(listId, cancellationToken))
            .ToDictionary(entity => entity.Id);
        var entities = new List<TEntity>(commands.Count);

        var missingId = listId.FirstOrDefault(id => !storedEntities.ContainsKey(id));
        if (missingId > 0)
            return Result<List<TEntity>>.Failure(NotFoundMessage(missingId));

        foreach (var command in commands)
        {
            var id = GetById(command);
            var entity = storedEntities[id];

            await ValidateBusinessRulesAsync(entity, command, cancellationToken);
            ApplyChanges(entity, command);
            entities.Add(entity);
        }

        Repository.UpdateRange(entities);
        await BeforeCommitAsync(entities, cancellationToken);
        await UnitOfWork.CommitAsync(cancellationToken);
        await AfterCommitAsync(entities, cancellationToken);

        return Result<List<TEntity>>.Success(entities);
    }
}
