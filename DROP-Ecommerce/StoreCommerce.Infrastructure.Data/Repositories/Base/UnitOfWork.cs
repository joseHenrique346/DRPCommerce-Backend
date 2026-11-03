using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class UnitOfWork : IIdentityUnitOfWork
{
    private readonly StoreCommerceDbContext _context;

    public UnitOfWork(StoreCommerceDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public IIdentityTransaction? CurrentTransaction
    {
        get
        {
            var transaction = _context.Database.CurrentTransaction;

            if (transaction is null)
                return null;

            return new IdentityTransaction(transaction);
        }
    }

    public async Task<IIdentityTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        return new IdentityTransaction(transaction);
    }

    async Task<IStoreTransaction> IUnitOfWork.BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await BeginTransactionAsync(cancellationToken);
    }
}
