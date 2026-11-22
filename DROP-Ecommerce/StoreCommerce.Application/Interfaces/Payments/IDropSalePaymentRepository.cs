using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Application.Contracts;

namespace StoreCommerce.Application.Interfaces.Payments;

public interface IDropSalePaymentRepository
{
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void ClearTracking();
    Task<Customer?> FindCustomerAsync(long customerId, long tenant, CancellationToken ct);
    Task<DropSalePaymentIntent?> ReadIntentAsync(long tenant, Guid operationId, CancellationToken ct);
    Task<DropSalePaymentIntent?> FindIntentAsync(long tenant, Guid operationId, CancellationToken ct);
    Task<List<DropSalePaymentIntent>> ListDueAsync(DateTime now, CancellationToken ct);
    Task LockCustomerAndTenantAsync(long tenant, long customerId, CancellationToken ct);
    Task LockTenantAsync(long tenant, CancellationToken ct);
    Task<List<DropSalePaymentIntent>> PendingReceiptsAsync(CancellationToken ct);
}
