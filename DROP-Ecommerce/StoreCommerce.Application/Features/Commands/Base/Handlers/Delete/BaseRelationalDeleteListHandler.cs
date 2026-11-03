using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands.Base.Handlers;

/// <summary>
/// Exclui fisicamente os registros sem vínculos e aplica exclusão lógica nos que possuem relacionamentos,
/// preservando o histórico (ex.: produtos já vendidos, categorias com produtos).
/// </summary>
public abstract class BaseRelationalDeleteListHandler<TRequest, TEntity>(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<TRequest, Result<bool>>
    where TRequest : IRequest<Result<bool>>
    where TEntity : BaseEntity, ISoftDeletable
{
    protected IRepository<TEntity> Repository { get; } = repository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract IReadOnlyCollection<long> GetIdList(TRequest request);

    /// <summary>
    /// Indica se o registro possui relacionamentos que impedem a exclusão física.
    /// </summary>
    protected abstract Task<bool> HasRelationsAsync(long id, CancellationToken cancellationToken);

    protected virtual string NotFoundMessage(long id) =>
        $"{typeof(TEntity).Name} com id {id} não encontrado.";

    public virtual async Task<Result<bool>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var listId = GetIdList(request).Distinct().ToList();
        var entities = (await Repository.GetListByListIdAsync(listId, cancellationToken)).ToDictionary(entity => entity.Id);

        var missingId = listId.FirstOrDefault(id => !entities.ContainsKey(id));
        if (missingId > 0)
            return Result<bool>.NotFound($"{typeof(TEntity).Name}.NotFound", NotFoundMessage(missingId));

        foreach (var entity in entities.Values)
        {
            if (await HasRelationsAsync(entity.Id, cancellationToken))
                entity.SoftDelete();
            else
                Repository.Remove(entity);
        }

        await UnitOfWork.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
