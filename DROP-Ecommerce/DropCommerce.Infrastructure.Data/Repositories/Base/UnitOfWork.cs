using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DropCommerceDbContext _context;

    public UnitOfWork(DropCommerceDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
