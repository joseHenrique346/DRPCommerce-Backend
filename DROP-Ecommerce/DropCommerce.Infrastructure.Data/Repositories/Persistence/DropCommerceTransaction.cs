using DropCommerce.Domain.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace DropCommerce.Infrastructure.Data.Repositories;

internal sealed class DropCommerceTransaction(IDbContextTransaction transaction) : IDropTransaction
{
    public Task CommitAsync(CancellationToken ct)
    {
        return transaction.CommitAsync(ct);
    }

    public ValueTask DisposeAsync()
    {
        return transaction.DisposeAsync();
    }
}
