using Microsoft.EntityFrameworkCore.Storage;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class IdentityTransaction(IDbContextTransaction transaction) : IIdentityTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return transaction.CommitAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return transaction.DisposeAsync();
    }
}
