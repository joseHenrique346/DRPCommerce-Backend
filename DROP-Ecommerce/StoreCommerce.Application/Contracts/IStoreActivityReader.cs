using StoreCommerce.Application.Contracts.Common;

namespace StoreCommerce.Application.Contracts;

public interface IStoreActivityReader
{
    Task<PagedResponse<StoreActivityResponse>> ReadAsync(
        long enterpriseId, StoreActivityQueryRequest request, CancellationToken cancellationToken);
}
