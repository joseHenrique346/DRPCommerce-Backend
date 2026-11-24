using DropCommerce.Application.Contracts;
using DropCommerce.Application.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Interfaces.Catalog;

public interface IDropCatalogRepository : IDropCommercePersistence
{
    Task<List<DropCatalogProduct>> ListProductsAsync(long? afterId, CancellationToken ct);
    Task<DropCatalogProduct?> FindProductWithVariantsAsync(long productId, CancellationToken ct);
    Task<DropEvent?> FindEventAsync(long eventId, CancellationToken ct);
    Task<DropProduct?> FindParticipationAsync(long eventId, long productId, CancellationToken ct);
    Task<List<DropCatalogProduct>> ListCatalogAsync(CancellationToken ct);
    Task<List<DropProduct>> ListParticipationsAsync(CancellationToken ct);
    Task<List<DropProduct>> ListActiveParticipationsAsync(CancellationToken ct);
    Task<DropCatalogProduct?> FindProductAsync(long productId, CancellationToken ct);
    Task<int> CountAvailableCatalogAsync(CancellationToken ct);
    Task<List<DropProduct>> ListProductParticipationsAsync(long productId, CancellationToken ct);
    Task<bool> HasReservationsAsync(List<long> ids, CancellationToken ct);
    Task<bool> HasPendingOrdersAsync(List<long> ids, CancellationToken ct);
    Task<List<DropCommercialActivity>> ListActivityAsync(long? beforeId, CancellationToken ct);
}
