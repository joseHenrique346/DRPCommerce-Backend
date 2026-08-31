using DropCommerce.Application.Integrations.Store.Contracts;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreProductReader
{
    Task<StoreProductData?> GetByIdAsync(long productId, long enterpriseId, CancellationToken cancellationToken);
    Task<IEnumerable<StoreProductData>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken);
}
