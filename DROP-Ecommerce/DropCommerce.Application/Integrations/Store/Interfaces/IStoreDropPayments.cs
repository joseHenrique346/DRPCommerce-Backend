using SharedCommerce.Application.Contracts.Drop;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreDropPayments
{
    Task<DropBuyerProfile> ProfileAsync(long tenant, long customerId, CancellationToken ct);
    Task<DropSalePaymentReceipt?> StartAsync(long tenant, StartDropSalePayment request, CancellationToken ct);
    Task<DropSalePaymentReceipt?> GetAsync(long tenant, Guid operationId, CancellationToken ct);
    Task AcknowledgeAsync(long tenant, Guid operationId, long revision, bool closed, bool requiresReview, CancellationToken ct);
}
