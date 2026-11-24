namespace DropCommerce.Domain.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
